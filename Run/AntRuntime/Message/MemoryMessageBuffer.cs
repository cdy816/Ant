using AntRuntime.Message;
using Cdy.Ant;
using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntRuntime
{

    /// <summary>
    /// 
    /// </summary>
    public class MemoryMessageHourBuffer:Dictionary<long, Cdy.Ant.Tag.Message>
    {

        /// <summary>
        /// 小时
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long LastFilePosition { get; set; } = 0;

        /// <summary>
        /// 最早消息的时间
        /// </summary>
        public DateTime OldestMessageTime { get; set; }

        /// <summary>
        /// 最近一条报警的时间
        /// </summary>
        public DateTime NewestMessageTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDirty { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void AddMessage(Cdy.Ant.Tag.Message msg)
        {
            this.Add(msg.Id, msg);
            IsDirty = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetId(DateTime id)
        {
            return id.Ticks * 10;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> Query(DateTime stime, DateTime etime)
        {
            long sid = GetId(stime);
            long eid = GetId(etime);
            return this.Where(e => e.Key >= sid && e.Key <= eid).Select(e => e.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> Query(DateTime stime, DateTime etime, IEnumerable<QueryFilter> Filters)
        {
            return Query(stime, etime).Filter(Filters);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MemoryMessageBuffer
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        private SortedDictionary<int,MemoryMessageHourBuffer> mBufferItems = new SortedDictionary<int, MemoryMessageHourBuffer>();

        private MemoryMessageHourBuffer mPreBuffer;

        private MemoryMessageHourBuffer mLastHourBuffer;

        /// <summary>
        /// 最早消息的时间
        /// </summary>
        public DateTime OldestMessageTime { get; set; }

        /// <summary>
        /// 最近一条报警的时间
        /// </summary>
        public DateTime NewestMessageTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DatabaseName { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void LoadBuffer()
        {
            DateTime dnow = DateTime.Now;
            var vv = new HisFileMessageBuffer() {AlarmDate = dnow, Starttime = dnow, Endtime = dnow.AddHours(1), DatabaseName = DatabaseName }.ReadDataBlockFromFile();
            if(vv!=null && vv.Count()>0)
            {
                mLastHourBuffer = new MemoryMessageHourBuffer() { Hour = dnow.Hour,LastFilePosition = vv.First().FilePosition };
                mLastHourBuffer.OldestMessageTime = dnow.Date.AddHours(dnow.Hour);

                foreach (var vvv in vv)
                {
                    foreach (var msg in vvv.GetMessages())
                    {
                        if (mLastHourBuffer.Count == 0)
                            mLastHourBuffer.OldestMessageTime = msg.CreateTime;
                        mLastHourBuffer.Add(msg.Id, msg);
                    }
                }

                lock (mBufferItems)
                    mBufferItems.Add(mLastHourBuffer.Hour,mLastHourBuffer);

                OldestMessageTime = mLastHourBuffer.OldestMessageTime;
            }
            else
            {
                OldestMessageTime = dnow;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void AddMessage(Cdy.Ant.Tag.Message msg)
        {
            
            NewestMessageTime = msg.CreateTime;

            int hh = msg.CreateTime.Hour;

            if(mLastHourBuffer==null )
            {
                mLastHourBuffer = new MemoryMessageHourBuffer() { Hour = hh };
                mLastHourBuffer.OldestMessageTime = msg.CreateTime;
                mLastHourBuffer.NewestMessageTime = msg.CreateTime;
                mLastHourBuffer.AddMessage(msg);
                
                OldestMessageTime = mLastHourBuffer.OldestMessageTime;
                NewestMessageTime = mLastHourBuffer.NewestMessageTime;

                lock (mBufferItems)
                    mBufferItems.Add(mLastHourBuffer.Hour,mLastHourBuffer);
            }
            else if(mLastHourBuffer.Hour == hh)
            {
                mLastHourBuffer.NewestMessageTime = msg.CreateTime;
                if(mLastHourBuffer.Count==0)
                {
                    mLastHourBuffer.OldestMessageTime = msg.CreateTime;
                }
                mLastHourBuffer.AddMessage(msg);

                NewestMessageTime = mLastHourBuffer.NewestMessageTime;
            }
            else
            {
                mPreBuffer = mLastHourBuffer;

                mLastHourBuffer = new MemoryMessageHourBuffer() { Hour = hh };
                mLastHourBuffer.OldestMessageTime = msg.CreateTime;
                mLastHourBuffer.NewestMessageTime = msg.CreateTime;
                NewestMessageTime = mLastHourBuffer.NewestMessageTime;
                mLastHourBuffer.AddMessage(msg);

                //进行存储
                Task.Run(() => {

                    MemoryMessageHourBuffer msavetarget = null;

                    lock (mBufferItems)
                    {
                        mBufferItems.Add(mLastHourBuffer.Hour, mLastHourBuffer);
                        msavetarget = mBufferItems.First().Value;
                    }

                    if(msavetarget != null)
                    CheckSaveAlarmToFile(msavetarget);
                });
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDataFile(DateTime Starttime)
        {
            string name = Starttime.ToString("yyyyMMdd");
            return PathHelper.helper.GetDataPath(DatabaseName, System.IO.Path.Combine("Alm", name + ".alm"));
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckSaveAlarmToFile(MemoryMessageHourBuffer buffer)
        {
            if (buffer.IsDirty)
            {
                string sfile = GetDataFile(buffer.OldestMessageTime);

                string sdir = System.IO.Path.GetDirectoryName(sfile);
                if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);

                using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
                {
                    if (buffer.LastFilePosition > 0)
                    {
                        vv.Position = buffer.LastFilePosition;
                    }
                    else
                    {
                        vv.Position = vv.Length;
                    }

                    MessageBlockBuffer mbb = new MessageBlockBuffer();
                    mbb.Hour = buffer.OldestMessageTime.Hour;
                    mbb.AlarmArea = new AlarmMessageAreaBuffer();
                    foreach (var vvm in buffer.Where(e => e.Value.Type == MessgeType.Alarm).Select(e => e.Value as AlarmMessage))
                    {
                        mbb.AlarmArea.AlarmMessage.Add(vvm.Id, vvm);
                    }

                    mbb.CommonArea = new CommonMessageAreaBuffer();
                    foreach (var vvm in buffer.Where(e => e.Value.Type == MessgeType.InfoMessage).Select(e => e.Value))
                        mbb.CommonArea.Message.Add(vvm.Id, vvm);

                    MessageFileSerise.Save(mbb, vv);
                }
                buffer.IsDirty = false;
            }

            lock (mBufferItems)
            {
                mBufferItems.Remove(buffer.Hour);
                if (mBufferItems.Count > 0)
                    OldestMessageTime = mBufferItems.First().Value.OldestMessageTime;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetId(DateTime id)
        {
            return id.Ticks * 10;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lid"></param>
        /// <returns></returns>
        public DateTime RestoreTimeFromId(long lid)
        {
            var vid = lid / 10;
            return DateTime.FromBinary(vid);
        }

        /// <summary>
        /// 恢复消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void RestoreMessage(long id, string value)
        {
            if (mLastHourBuffer != null && mLastHourBuffer.ContainsKey(id))
            {
                var tmp = mLastHourBuffer[id];
                (tmp as AlarmMessage).RestoreTime = DateTime.Now;
                (tmp as AlarmMessage).RestoreValue = value;
                mLastHourBuffer.IsDirty = true;
            }
            else
            {
                DateTime dt = RestoreTimeFromId(id);

                lock (mBufferItems)
                {
                    if (mBufferItems.ContainsKey(dt.Hour))
                    {
                        var vbmp = mBufferItems[dt.Hour];
                        if (vbmp.ContainsKey(id))
                        {
                            var tmp = vbmp[id];
                            (tmp as AlarmMessage).RestoreTime = DateTime.Now;
                            (tmp as AlarmMessage).RestoreValue = value;
                            vbmp.IsDirty = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  确认消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="user"></param>
        public void AckMessage(long id,string content,string user)
        {
            if (mLastHourBuffer!=null && mLastHourBuffer.ContainsKey(id))
            {
                var tmp = mLastHourBuffer[id];
                (tmp as AlarmMessage).AckTime = DateTime.Now;
                (tmp as AlarmMessage).AckUser = user;
                (tmp as AlarmMessage).AckMessage = content;
                mLastHourBuffer.IsDirty = true;
            }
            else
            {
                DateTime dt = RestoreTimeFromId(id);

                lock (mBufferItems)
                {
                    if (mBufferItems.ContainsKey(dt.Hour))
                    {
                        var vbmp = mBufferItems[dt.Hour];
                        if (vbmp.ContainsKey(id))
                        {
                            var tmp = vbmp[id];
                            (tmp as AlarmMessage).AckTime = DateTime.Now;
                            (tmp as AlarmMessage).AckUser = user;
                            (tmp as AlarmMessage).AckMessage = content;
                            vbmp.IsDirty = true;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="user"></param>
        public void DeleteMessage(long id, string content, string user)
        {
            if (mLastHourBuffer != null && mLastHourBuffer.ContainsKey(id))
            {
                var tmp = mLastHourBuffer[id];
                (tmp as AlarmMessage).DeleteTime = DateTime.Now;
                (tmp as AlarmMessage).DeleteUser = user;
                (tmp as AlarmMessage).DeleteNote = content;
                mLastHourBuffer.IsDirty = true;
            }
            else
            {
                DateTime dt = RestoreTimeFromId(id);

                lock (mBufferItems)
                {
                    if (mBufferItems.ContainsKey(dt.Hour))
                    {
                        var vbmp = mBufferItems[dt.Hour];
                        if (vbmp.ContainsKey(id))
                        {
                            var tmp = vbmp[id];
                            (tmp as AlarmMessage).DeleteTime = DateTime.Now;
                            (tmp as AlarmMessage).DeleteUser = user;
                            (tmp as AlarmMessage).DeleteNote = content;
                            vbmp.IsDirty = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 将变脏的数据块存盘
        /// </summary>
        public void FlushDirtyBufferToDisk()
        {
            foreach(var vv in mBufferItems.ToArray())
            {
                if(vv.Value.IsDirty)
                {
                    CheckSaveAlarmToFile(vv.Value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Cdy.Ant.Tag.Message Query(long id)
        {
            if(mLastHourBuffer != null && mLastHourBuffer.ContainsKey(id))
            {
                return mLastHourBuffer[id];
            }
            else
            {
                DateTime dt = RestoreTimeFromId(id);

                lock (mBufferItems)
                {
                    if (mBufferItems.ContainsKey(dt.Hour))
                    {
                        var vbmp = mBufferItems[dt.Hour];
                        if (vbmp.ContainsKey(id))
                        {
                            return vbmp[id];
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> Query(DateTime stime, DateTime etime)
        {
            long sid = GetId(stime);
            long eid = GetId(etime);

            List<Cdy.Ant.Tag.Message> re = new List<Cdy.Ant.Tag.Message>();

            double endhour = (etime - stime).TotalHours;

            List<int> hours = new List<int>();

            for (int i = 0; i <= endhour; i++)
            {
                hours.Add(stime.Hour + i);
            }

            MemoryMessageHourBuffer mbuffer;

            foreach(var vv in hours)
            {
                lock(mBufferItems)
                {
                    if(mBufferItems.ContainsKey(vv))
                    {
                        mbuffer = mBufferItems[vv];
                    }
                    else
                    {
                        mbuffer = null;
                    }
                }

                if(mbuffer != null)
                {
                    re.AddRange(mbuffer.Where(e => e.Key >= sid && e.Key <= eid && e.Value.DeleteTime == DateTime.MinValue).Select(e => e.Value));
                }

            }
            return re;
        }


        /// <summary>
        /// 查询所有消息，包括删除消息
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> QueryAll(DateTime stime, DateTime etime)
        {
            long sid = GetId(stime);
            long eid = GetId(etime);

            List<Cdy.Ant.Tag.Message> re = new List<Cdy.Ant.Tag.Message>();

            double endhour = (etime - stime).TotalHours;

            List<int> hours = new List<int>();

            for (int i = 0; i <= endhour; i++)
            {
                hours.Add(stime.Hour+i);
            }

            MemoryMessageHourBuffer mbuffer;

            foreach (var vv in hours)
            {
                lock (mBufferItems)
                {
                    if (mBufferItems.ContainsKey(vv))
                    {
                        mbuffer = mBufferItems[vv];
                    }
                    else
                    {
                        mbuffer = null;
                    }
                }

                if (mbuffer != null)
                {
                    re.AddRange(mbuffer.Where(e => e.Key >= sid && e.Key <= eid).Select(e => e.Value));
                }

            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> Query(DateTime stime, DateTime etime, IEnumerable<QueryFilter> Filters)
        {
            return Query(stime,etime).Filter(Filters);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> QueryAll(DateTime stime, DateTime etime, IEnumerable<QueryFilter> Filters)
        {
            return QueryAll(stime, etime).Filter(Filters);
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
