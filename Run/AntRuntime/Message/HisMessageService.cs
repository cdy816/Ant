using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntRuntime
{
    /// <summary>
    /// 历史报警查询
    /// </summary>
    public class HisMessageService
    {

        #region ... Variables  ...
        /// <summary>
        /// 
        /// </summary>
        public static HisMessageService Service = new HisMessageService();

        private object mLockObj = new object();

        private Thread mScanThread;

        private bool mIsClosed = false;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        private Dictionary<DateTime, HisFileMessageBuffer> mBufferedFiles = new Dictionary<DateTime, HisFileMessageBuffer>();

        /// <summary>
        /// 
        /// </summary>
        public string DatabaseName { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            mScanThread = new Thread(OldBufferRemoveProcess);
            mScanThread.IsBackground = true;
            mScanThread.Start();
        }


        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            mIsClosed = true;
            while (mScanThread.IsAlive) Thread.Sleep(1);
            SaveChangedMessageToDisk();
        }

        /// <summary>
        /// 删除长时间不实用的缓存
        /// </summary>
        public void OldBufferRemoveProcess()
        {
            while(!mIsClosed)
            {
                DateTime dnow = DateTime.Now;
                List<DateTime> mremoved = new List<DateTime>();
                lock (mLockObj)
                {
                    foreach (var vv in mBufferedFiles)
                    {
                        if ((dnow - vv.Value.LastAccessTime).TotalDays > 1)
                        {
                            mremoved.Add(vv.Key);
                        }
                    }
                }

                lock (mLockObj)
                {
                    foreach (var vv in mremoved)
                    {
                        mBufferedFiles[vv].SaveChangedMessageToDisk();
                        mBufferedFiles.Remove(vv);
                    }
                }
                Thread.Sleep(10000);
            }
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void RestoreMessage(long id, string value)
        {
            DateTime dt = RestoreTimeFromId(id);
            var dtt = dt.Date;

            if(mBufferedFiles.ContainsKey(dtt))
            {
                mBufferedFiles[dtt].RestoreMessage(id, value, dt.Hour);
            }
            else
            {
                var vv = new HisFileMessageBuffer() { Starttime = dt, Endtime = dt.AddHours(1), DatabaseName = DatabaseName, AlarmDate = dtt };
                mBufferedFiles.Add(vv.AlarmDate, vv);
                vv.RestoreMessage(id, value, dt.Hour);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="user"></param>
        public void AckMessage(long id, string content, string user)
        {
            DateTime dt = RestoreTimeFromId(id);
            var dtt = dt.Date;
            lock (mLockObj)
            {
                if (mBufferedFiles.ContainsKey(dtt))
                {
                    mBufferedFiles[dtt].AckMessage(id, user, content, dt.Hour);
                }
                else
                {
                    var vv = new HisFileMessageBuffer() { Starttime = dt, Endtime = dt.AddHours(1), DatabaseName = DatabaseName, AlarmDate = dtt };
                    mBufferedFiles.Add(vv.AlarmDate, vv);
                    vv.AckMessage(id, user, content, dt.Hour);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="user"></param>
        public void DeleteMessage(long id, string content, string user)
        {
            DateTime dt = RestoreTimeFromId(id);
            var dtt = dt.Date;
            lock (mLockObj)
            {
                if (mBufferedFiles.ContainsKey(dtt))
                {
                    mBufferedFiles[dtt].DeleteMessage(id, user, content, dt.Hour);
                }
                else
                {
                    var vv = new HisFileMessageBuffer() { Starttime = dt, Endtime = dt.AddHours(1), DatabaseName = DatabaseName, AlarmDate = dtt };
                    mBufferedFiles.Add(vv.AlarmDate, vv);
                    vv.DeleteMessage(id, user, content, dt.Hour);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lid"></param>
        public Cdy.Ant.Message Query(long lid)
        {
            HisFileMessageBuffer hh;
            var time = RestoreTimeFromId(lid);
            lock (mLockObj)
            {
                if (mBufferedFiles.ContainsKey(time.Date))
                {
                    hh = mBufferedFiles[time.Date];
                }
                else
                {
                    var vv = new HisFileMessageBuffer() { Starttime = time, Endtime = time.AddHours(1), DatabaseName = DatabaseName, AlarmDate = time.Date };
                    mBufferedFiles.Add(vv.AlarmDate, vv);
                    vv.LoadMessageBlock(time.Hour);
                    hh = vv;
                }
            }
            return hh.Query(lid,time.Hour);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> Query(DateTime stime,DateTime etime, IEnumerable<QueryFilter> Filters)
        {
            var re = Query(stime, etime);
            if (Filters != null && Filters.Count() > 0)
                return re.Filter(Filters);
            else
                return re;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryAll(DateTime stime, DateTime etime, IEnumerable<QueryFilter> Filters)
        {
            var re = QueryAll(stime, etime);
            if (Filters != null && Filters.Count() > 0)
                return re.Filter(Filters);
            else
                return re;
        }

        /// <summary>
        /// 查询报警、踢出到以删除的报警
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> Query(DateTime stime, DateTime etime)
        {
            Dictionary<HisFileMessageBuffer,Tuple<DateTime,DateTime>> mReaders = new Dictionary<HisFileMessageBuffer, Tuple<DateTime, DateTime>>();

            DateTime st = stime;
            while(st.Day<=etime.Day)
            {
                if(st.Day == etime.Day)
                {
                    if (mBufferedFiles.ContainsKey(st.Date))
                    {
                        var vv = mBufferedFiles[st.Date];
                        mReaders.Add(vv,new Tuple<DateTime, DateTime>(st,etime));
                    }
                    else
                    {
                        lock (mLockObj)
                        {
                            var vv = new HisFileMessageBuffer() { Starttime = st, Endtime = etime, DatabaseName = DatabaseName, AlarmDate = st.Date, LastAccessTime = DateTime.Now };
                            mBufferedFiles.Add(vv.AlarmDate, vv);
                            mReaders.Add(vv, new Tuple<DateTime, DateTime>(st, etime));
                        }
                    }
                    break;
                }
                else
                {
                    if (mBufferedFiles.ContainsKey(st.Date))
                    {
                        var vv = mBufferedFiles[st.Date];
                        mReaders.Add(vv,new Tuple<DateTime, DateTime>(st,st.Date.AddDays(1)));
                    }
                    else
                    {
                        lock (mLockObj)
                        {
                            var vv = new HisFileMessageBuffer() { Starttime = st, Endtime = st.Date.AddDays(1), DatabaseName = DatabaseName, AlarmDate = st.Date, LastAccessTime = DateTime.Now };
                            mBufferedFiles.Add(vv.AlarmDate, vv);
                            mReaders.Add(vv, new Tuple<DateTime, DateTime>(st, st.Date.AddDays(1)));
                        }
                    }
                }
                st = st.Date.AddDays(1);
            }
            List<Cdy.Ant.Message> re = new List<Cdy.Ant.Message>();
            foreach (var vv in mReaders)
            {
                var vtmp = vv.Key.ReadFromFile(vv.Value.Item1,vv.Value.Item2);
                if (vtmp != null)
                {
                    re.AddRange(vtmp.Where(e=>e.DeleteTime==DateTime.MinValue));
                }
            }

            return re;
        }


        /// <summary>
        ///  查询所有报警
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryAll(DateTime stime, DateTime etime)
        {
            Dictionary<HisFileMessageBuffer, Tuple<DateTime, DateTime>> mReaders = new Dictionary<HisFileMessageBuffer, Tuple<DateTime, DateTime>>();

            DateTime st = stime;
            while (st.Day <= etime.Day)
            {
                if (st.Day == etime.Day)
                {
                    if (mBufferedFiles.ContainsKey(st.Date))
                    {
                        var vv = mBufferedFiles[st.Date];
                        mReaders.Add(vv, new Tuple<DateTime, DateTime>(st, etime));
                    }
                    else
                    {
                        lock (mLockObj)
                        {
                            var vv = new HisFileMessageBuffer() { Starttime = st, Endtime = etime, DatabaseName = DatabaseName, AlarmDate = st.Date, LastAccessTime = DateTime.Now };
                            mBufferedFiles.Add(vv.AlarmDate, vv);
                            mReaders.Add(vv, new Tuple<DateTime, DateTime>(st, etime));
                        }
                    }
                    break;
                }
                else
                {
                    if (mBufferedFiles.ContainsKey(st.Date))
                    {
                        var vv = mBufferedFiles[st.Date];
                        mReaders.Add(vv, new Tuple<DateTime, DateTime>(st, st.Date.AddDays(1)));
                    }
                    else
                    {
                        lock (mLockObj)
                        {
                            var vv = new HisFileMessageBuffer() { Starttime = st, Endtime = st.Date.AddDays(1), DatabaseName = DatabaseName, AlarmDate = st.Date, LastAccessTime = DateTime.Now };
                            mBufferedFiles.Add(vv.AlarmDate, vv);
                            mReaders.Add(vv, new Tuple<DateTime, DateTime>(st, st.Date.AddDays(1)));
                        }
                    }
                }
                st = st.Date.AddDays(1);
            }
            List<Cdy.Ant.Message> re = new List<Cdy.Ant.Message>();
            foreach (var vv in mReaders)
            {
                var vtmp = vv.Key.ReadFromFile(vv.Value.Item1, vv.Value.Item2);
                if (vtmp != null)
                {
                    re.AddRange(vtmp);
                }
            }

            return re;
        }


        /// <summary>
        /// 
        /// </summary>
        public void SaveChangedMessageToDisk()
        {
            foreach(var vv in mBufferedFiles)
            {
                vv.Value.SaveChangedMessageToDisk();
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public static class HisQueryExtends
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public static IEnumerable<Cdy.Ant.Message> Filter(this IEnumerable<Cdy.Ant.Message> messages, IEnumerable<QueryFilter> Filters)
        {
            if (messages == null) return null;
            if (Filters == null || Filters.Count() == 0) return messages;

            return messages.Where((e) => {
                bool re = true;
                foreach(var vv in Filters)
                {
                    switch(vv.PropertyName)
                    {
                        case "Type":
                            re &= (e.Type.ToString() == vv.Value);
                            break;
                        case "Server":
                            switch (vv.Opetate)
                            {
                                case FilterOperate.Contains:
                                    re &= e.Server.Contains(vv.Value);
                                    break;
                                case FilterOperate.Equals:
                                    re &= e.Server == vv.Value;
                                    break;
                                default:
                                    re = false;
                                    break;
                            }
                            break;
                        case "SourceTag":
                            switch (vv.Opetate)
                            {
                                case FilterOperate.Contains:
                                    re &= e.SourceTag.Contains(vv.Value);
                                    break;
                                case FilterOperate.Equals:
                                    re &= e.SourceTag == vv.Value;
                                    break;
                                default:
                                    re = false;
                                    break;
                            }
                            break;
                        case "MessageBody":
                            switch (vv.Opetate)
                            {
                                case FilterOperate.Contains:
                                    re &= e.MessageBody.Contains(vv.Value);
                                    break;
                                case FilterOperate.Equals:
                                    re &= e.MessageBody == vv.Value;
                                    break;
                                default:
                                    re = false;
                                    break;
                            }
                            break;
                        case "AppendContent1":
                            switch (vv.Opetate)
                            {
                                case FilterOperate.Contains:
                                    re &= e.AppendContent1.Contains(vv.Value);
                                    break;
                                case FilterOperate.Equals:
                                    re &= e.AppendContent1 == vv.Value;
                                    break;
                                default:
                                    re = false;
                                    break;
                            }
                            break;
                        case "AppendContent2":
                            switch (vv.Opetate)
                            {
                                case FilterOperate.Contains:
                                    re &= e.AppendContent2.Contains(vv.Value);
                                    break;
                                case FilterOperate.Equals:
                                    re &= e.AppendContent2 == vv.Value;
                                    break;
                                default:
                                    re = false;
                                    break;
                            }
                            break;
                        case "AppendContent3":
                            switch (vv.Opetate)
                            {
                                case FilterOperate.Contains:
                                    re &= e.AppendContent3.Contains(vv.Value);
                                    break;
                                case FilterOperate.Equals:
                                    re &= e.AppendContent3 == vv.Value;
                                    break;
                                default:
                                    re = false;
                                    break;
                            }
                            break;
                        case "DisposalMessages":
                            if (e.DisposalMessages.Count > 0)
                            {
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        bool rtmp = false;
                                        foreach (var vvv in e.DisposalMessages)
                                        {
                                            if (vvv.Message.Contains(vv.Value))
                                            {
                                                rtmp = true;
                                                break;
                                            }
                                        }
                                        re &= rtmp;
                                        break;
                                    case FilterOperate.Equals:
                                        rtmp = false;
                                        foreach (var vvv in e.DisposalMessages)
                                        {
                                            if (vvv.Message == (vv.Value))
                                            {
                                                rtmp = true;
                                                break;
                                            }
                                        }
                                        re &= rtmp;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re &= false;
                            }
                            break;
                        case "DisposalMessages.User":
                            if (e.DisposalMessages.Count > 0)
                            {
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        bool rtmp = false;
                                        foreach (var vvv in e.DisposalMessages)
                                        {
                                            if (vvv.User.Contains(vv.Value))
                                            {
                                                rtmp = true;
                                                break;
                                            }
                                        }
                                        re &= rtmp;
                                        break;
                                    case FilterOperate.Equals:
                                        rtmp = false;
                                        foreach (var vvv in e.DisposalMessages)
                                        {
                                            if (vvv.User == (vv.Value))
                                            {
                                                rtmp = true;
                                                break;
                                            }
                                        }
                                        re &= rtmp;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re &= false;
                            }
                            break;
                        case "DisposalMessages.Time":
                            if (e.DisposalMessages.Count > 0)
                            {
                                DateTime dt = DateTime.Parse(vv.Value);
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Low:
                                        bool rtmp = false;
                                        foreach (var vvv in e.DisposalMessages)
                                        {
                                            if (vvv.Time < dt)
                                            {
                                                rtmp = true;
                                                break;
                                            }
                                        }
                                        re &= rtmp;
                                        break;
                                    case FilterOperate.Great:
                                        rtmp = false;
                                        foreach (var vvv in e.DisposalMessages)
                                        {
                                            if (vvv.Time > dt)
                                            {
                                                rtmp = true;
                                                break;
                                            }
                                        }
                                        re &= rtmp;
                                        break;
                                    case FilterOperate.Equals:
                                        rtmp = false;
                                        foreach (var vvv in e.DisposalMessages)
                                        {
                                            if (vvv.Time == dt)
                                            {
                                                rtmp = true;
                                                break;
                                            }
                                        }
                                        re &= rtmp;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re &= false;
                            }
                            break;
                        case "AlarmLevel":
                            if(e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.AlarmLevel.ToString();
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        re &= als.Contains(vv.Value);
                                        break;
                                    case FilterOperate.Equals:
                                        re &= als == vv.Value;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "AlarmValue":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.AlarmValue.ToString();
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        re &= als.Contains(vv.Value);
                                        break;
                                    case FilterOperate.Equals:
                                        re &= als == vv.Value;
                                        break;
                                    case FilterOperate.Great:
                                        if(IsNumber(als,out double v1) && IsNumber(vv.Value,out double v2))
                                        {
                                            re &= v1 > v2;
                                        }
                                        else
                                        {
                                            re = false;
                                        }
                                        break;
                                    case FilterOperate.Low:
                                        if (IsNumber(als, out double v11) && IsNumber(vv.Value, out double v12))
                                        {
                                            re &= v11 < v12;
                                        }
                                        else
                                        {
                                            re = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "AlarmCondition":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.AlarmCondition.ToString();
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        re &= als.Contains(vv.Value);
                                        break;
                                    case FilterOperate.Equals:
                                        re &= als == vv.Value;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "LinkTag":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.LinkTag.ToString();
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        re &= als.Contains(vv.Value);
                                        break;
                                    case FilterOperate.Equals:
                                        re &= als == vv.Value;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "AckMessage":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.AckMessage.ToString();
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        re &= als.Contains(vv.Value);
                                        break;
                                    case FilterOperate.Equals:
                                        re &= als == vv.Value;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "AckUser":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.AckUser.ToString();
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        re &= als.Contains(vv.Value);
                                        break;
                                    case FilterOperate.Equals:
                                        re &= als == vv.Value;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "AckTime":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.AckTime;
                                
                                var dt = DateTime.Parse(vv.Value);
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Equals:
                                        re &= als == dt;
                                        break;
                                    case FilterOperate.Great:
                                        re &= als > dt;
                                        break;
                                    case FilterOperate.Low:
                                        re &= als < dt;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "RestoreTime":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.RestoreTime;
                                var dt = DateTime.Parse(vv.Value);
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Equals:
                                        re &= als == dt;
                                        break;
                                    case FilterOperate.Great:
                                        re &= als > dt;
                                        break;
                                    case FilterOperate.Low:
                                        re &= als < dt;
                                        break;
                                    default:
                                        re = false;
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                            }
                            break;
                        case "RestoreValue":
                            if (e is Cdy.Ant.AlarmMessage)
                            {
                                var alm = e as Cdy.Ant.AlarmMessage;
                                var als = alm.RestoreValue.ToString();
                                switch (vv.Opetate)
                                {
                                    case FilterOperate.Contains:
                                        re &= als.Contains(vv.Value);
                                        break;
                                    case FilterOperate.Equals:
                                        re &= als == vv.Value;
                                        break;
                                    case FilterOperate.Great:
                                        if (IsNumber(als, out double v1) && IsNumber(vv.Value, out double v2))
                                        {
                                            re &= v1 > v2;
                                        }
                                        else
                                        {
                                            re = false;
                                        }
                                        break;
                                    case FilterOperate.Low:
                                        if (IsNumber(als, out double v11) && IsNumber(vv.Value, out double v12))
                                        {
                                            re &= v11 < v12;
                                        }
                                        else
                                        {
                                            re = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                re = false;
                                
                            }
                            break;
                    }
                    if (!re) break;
                }
                return re;
            }); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sval"></param>
        /// <param name="re"></param>
        /// <returns></returns>
        public static bool IsNumber(string sval,out double re)
        {
            return double.TryParse(sval, out re);
        }
    }
}
