﻿using AntRuntime.Message;
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
            using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                foreach (var vvv in ltmp)
                {
                    if (vvv.AlarmArea.IsAckDirty || vvv.AlarmArea.IsRestoreDirty)
                    {
                        MessageFileSerise.UpdateDirtyToDisk(vvv, vv);
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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDataFile()
        {
            string name = AlarmDate.ToString("yyyyMMdd");
            return PathHelper.helper.GetDataPath(DatabaseName, name + ".alm");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> ReadFromFile()
        {
            return ReadFromFile(Starttime, Endtime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        public MessageBlockBuffer ReadMessgeBlock(int hour)
        {
            string sfile = GetDataFile();
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
        public Cdy.Ant.Message Query(long id,int hour)
        {
            if(mBuffers.ContainsKey(hour))
            {
                var bb = mBuffers[hour];
                if(bb.AlarmArea.AlarmMessage.ContainsKey(id))
                {
                    return bb.AlarmArea.AlarmMessage[id];
                }
                var res = bb.CommonArea.Message.Where(e => e.Id == id);
                if (res.Count() > 0) return res.First();
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
                mBuffers.Add(hour, vv);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> ReadFromFile(DateTime startTime, DateTime endTime)
        {
            string sfile = GetDataFile();
            List<Cdy.Ant.Message> ll = new List<Cdy.Ant.Message>();
            if (System.IO.File.Exists(sfile))
            {
                using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {

                    Dictionary<int, MessageBlockBuffer> dd = new Dictionary<int, MessageBlockBuffer>();
                    List<int> itmp = new List<int>();
                    for (int i = startTime.Hour; i < endTime.Hour; i++)
                    {
                        if (!mBuffers.ContainsKey(i))
                        {
                            itmp.Add(i);
                        }
                    }

                    if (itmp.Count > 0)
                    {
                        foreach (var vvb in new MessageFileSerise().Load(vv, itmp).Result)
                        {
                            mBuffers.Add(vvb.Hour, vvb);
                        }
                    }

                    for (int i = Starttime.Hour; i < Endtime.Hour; i++)
                    {
                        ll.AddRange(mBuffers[i].GetMessages(startTime, endTime));
                    }
                }
            }
            return ll;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            SaveChangedMessageToDisk();
            foreach(var vv in mBuffers)
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