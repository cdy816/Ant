using AntRuntime.Message;
using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime
{
    /// <summary>
    /// 
    /// </summary>
    public class HisFileMessageBuffer:IDisposable
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public DateTime AlarmDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Starttime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Endtime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<int,MessageBlockBuffer> mBuffers = new Dictionary<int, MessageBlockBuffer>();

        /// <summary>
        /// 最后一次访问时间
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 将变动内容存盘
        /// </summary>
        public void SaveChangedMessageToDisk()
        {
            List<MessageBlockBuffer> ltmp;
            lock (mBuffers)
                ltmp = mBuffers.Values.ToList();
            string sfile = GetDataFile();
            string sdir = System.IO.Path.GetDirectoryName(sfile);
            if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);

            if (ltmp.Count > 0)
            {
                using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
                {
                    foreach (var vvv in ltmp)
                    {
                        if ((vvv.AlarmArea != null && (vvv.AlarmArea.IsAckDirty || vvv.AlarmArea.IsRestoreDirty || vvv.AlarmArea.IsDeleteDirty))|| (vvv.CommonArea != null && vvv.CommonArea.IsDeleteDirty))
                        {
                            MessageFileSerise.UpdateDirtyToDisk(vvv, vv);
                            vvv.AlarmArea.IsAckDirty = vvv.AlarmArea.IsRestoreDirty = vvv.AlarmArea.IsDeleteDirty = false;
                            if(vvv.CommonArea!=null) vvv.CommonArea.IsDeleteDirty = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="content"></param>
        /// <param name="hour"></param>
        public void AckMessage(long id,string user,string content,int hour)
        {
            if(mBuffers.ContainsKey(hour))
            {
                var msg = mBuffers[hour].AlarmArea.AlarmMessage;
                if(msg.ContainsKey(id))
                {
                    var mg = msg[id];
                    mg.AckMessage = content;
                    mg.AckTime = DateTime.Now;
                    mg.AckUser = user;
                }
                mBuffers[hour].AlarmArea.IsAckDirty = true;
                LastAccessTime = DateTime.Now;
            }
            else
            {
                var bb = ReadMessgeBlock(hour);
                if (bb == null) return;

                mBuffers.Add(hour, bb);

                var msg = mBuffers[hour].AlarmArea.AlarmMessage;
                if (msg.ContainsKey(id))
                {
                    var mg = msg[id];
                    mg.AckMessage = content;
                    mg.AckTime = DateTime.Now;
                    mg.AckUser = user;
                }
                mBuffers[hour].AlarmArea.IsAckDirty = true;
                LastAccessTime = DateTime.Now;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <param name="content"></param>
        /// <param name="hour"></param>
        public void DeleteMessage(long id, string user, string content, int hour)
        {
            if (mBuffers.ContainsKey(hour))
            {
                var msg = mBuffers[hour].AlarmArea.AlarmMessage;
                if (msg.ContainsKey(id))
                {
                    var mg = msg[id];
                    mg.DeleteNote = content;
                    mg.DeleteTime = DateTime.Now;
                    mg.DeleteUser = user;
                    mBuffers[hour].AlarmArea.IsDeleteDirty = true;
                }
                else if(mBuffers[hour].CommonArea.Message.ContainsKey(id))
                {
                    var mg = mBuffers[hour].CommonArea.Message[id];
                    mg.DeleteNote = content;
                    mg.DeleteTime = DateTime.Now;
                    mg.DeleteUser = user;
                    mBuffers[hour].CommonArea.IsDeleteDirty = true;
                }
               
                LastAccessTime = DateTime.Now;
            }
            else
            {
                var bb = ReadMessgeBlock(hour);
                if (bb == null) return;

                mBuffers.Add(hour, bb);

                var msg = mBuffers[hour].AlarmArea.AlarmMessage;
                if (msg.ContainsKey(id))
                {
                    var mg = msg[id];
                    mg.AckMessage = content;
                    mg.AckTime = DateTime.Now;
                    mg.AckUser = user;
                    mBuffers[hour].AlarmArea.IsDeleteDirty = true;
                }
                else if (mBuffers[hour].CommonArea.Message.ContainsKey(id))
                {
                    var mg = mBuffers[hour].CommonArea.Message[id];
                    mg.DeleteNote = content;
                    mg.DeleteTime = DateTime.Now;
                    mg.DeleteUser = user;
                    mBuffers[hour].CommonArea.IsDeleteDirty = true;
                }
               
                LastAccessTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="hour"></param>
        public void RestoreMessage(long id,string content,int hour)
        {
            if (mBuffers.ContainsKey(hour))
            {
                var msg = mBuffers[hour].AlarmArea.AlarmMessage;
                if (msg.ContainsKey(id))
                {
                    var mg = msg[id];
                    mg.RestoreValue = content;
                    mg.RestoreTime = DateTime.Now;
                }
                mBuffers[hour].AlarmArea.IsRestoreDirty = true;
                LastAccessTime = DateTime.Now;
            }
            else
            {
                var bb = ReadMessgeBlock(hour);
                if (bb == null) return;

                mBuffers.Add(hour, bb);

                var msg = mBuffers[hour].AlarmArea.AlarmMessage;
                if (msg.ContainsKey(id))
                {
                    var mg = msg[id];
                    mg.RestoreValue = content;
                    mg.RestoreTime = DateTime.Now;
                }
                mBuffers[hour].AlarmArea.IsRestoreDirty = true;
                LastAccessTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDataFile()
        {
            string name = AlarmDate.ToString("yyyyMMdd");
            return PathHelper.helper.GetDataPath(DatabaseName,System.IO.Path.Combine("Alm", name + ".alm"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> ReadFromFile()
        {
            return ReadFromFile(Starttime, Endtime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MessageBlockBuffer> ReadDataBlockFromFile()
        {
            return ReadDataBlockFromFile(Starttime, Endtime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        public MessageBlockBuffer ReadMessgeBlock(int hour)
        {
            string sfile = GetDataFile();
            string sdir = System.IO.Path.GetDirectoryName(sfile);
            if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);

            if (System.IO.File.Exists(sfile))
            {
                using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    return new MessageFileSerise().Load(vv, new List<int>() { hour }).Result[0];
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Cdy.Ant.Tag.Message Query(long id,int hour)
        {
            lock (mBuffers)
            {
                if (mBuffers.ContainsKey(hour))
                {
                    var bb = mBuffers[hour];
                    if (bb.AlarmArea.AlarmMessage.ContainsKey(id))
                    {
                        return bb.AlarmArea.AlarmMessage[id];
                    }

                    if (bb.CommonArea.Message.ContainsKey(id))
                    {
                        return bb.CommonArea.Message[id];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hour"></param>
        public void LoadMessageBlock(int hour)
        {
            var vv = ReadMessgeBlock(hour);
            if(vv!=null)
            {
                lock(mBuffers)
                mBuffers.Add(hour, vv);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IEnumerable<MessageBlockBuffer> ReadDataBlockFromFile(DateTime startTime, DateTime endTime)
        {
            string sfile = GetDataFile();
            string sdir = System.IO.Path.GetDirectoryName(sfile);
            if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);

            Dictionary<int, MessageBlockBuffer> dd = new Dictionary<int, MessageBlockBuffer>();
            if (System.IO.File.Exists(sfile))
            {
                using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {

                    double endhour = (endTime - startTime).TotalHours;

                    List<int> itmp = new List<int>();

                    for (int i = 0; i <= endhour; i++)
                    {
                        if (!dd.ContainsKey(startTime.Hour+i))
                        {
                            itmp.Add(startTime.Hour + i);
                        }
                    }

                    if (itmp.Count > 0)
                    {
                        foreach (var vvb in new MessageFileSerise().Load(vv, itmp).Result)
                        {
                            dd.Add(vvb.Hour, vvb);
                        }
                    }
                }
                
                LastAccessTime = DateTime.Now;
            }
            return dd.Values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> ReadFromFile(DateTime startTime, DateTime endTime)
        {
            string sfile = GetDataFile();
            string sdir = System.IO.Path.GetDirectoryName(sfile);
            if (!System.IO.Directory.Exists(sdir)) System.IO.Directory.CreateDirectory(sdir);

            List<Cdy.Ant.Tag.Message> ll = new List<Cdy.Ant.Tag.Message>();
            if (System.IO.File.Exists(sfile))
            {
                using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {


                    double endhour = (endTime - startTime).TotalHours;

                    List<int> itmp = new List<int>();

                    for (int i = 0; i <= endhour; i++)
                    {
                        if (!mBuffers.ContainsKey(startTime.Hour + i))
                        {
                            itmp.Add(startTime.Hour + i);
                        }
                    }

                    if (itmp.Count > 0)
                    {
                        foreach (var vvb in new MessageFileSerise().Load(vv, itmp).Result)
                        {
                            if (!mBuffers.ContainsKey(vvb.Hour))
                                mBuffers.Add(vvb.Hour, vvb);
                        }
                    }

                    for (int i = 0; i <= endhour; i++)
                    {
                        if(mBuffers.ContainsKey(Starttime.Hour+i))
                        ll.AddRange(mBuffers[Starttime.Hour+i].GetMessages(startTime, endTime));
                    }
                }
                LastAccessTime = DateTime.Now;
            }
            return ll;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            SaveChangedMessageToDisk();
            foreach (var vv in mBuffers)
            {
                vv.Value.Dispose();
            }
            mBuffers.Clear();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
