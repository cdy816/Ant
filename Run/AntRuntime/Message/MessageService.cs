using AntRuntime.Message;
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
    public class MessageService: IMessageQuery
    {

        #region ... Variables  ...
        
        /// <summary>
        /// 
        /// </summary>
        public static MessageService Service = new MessageService();

        private long mLastId = 0;

        private int mCount = 0;

        private object mLockObj = new object();

        private MemoryMessageBuffer mMemoryBuffer = new MemoryMessageBuffer();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
            /// 
            /// </summary>
        public string DatabaseName
        {
            get
            {
                return mMemoryBuffer.DatabaseName;
            }
            set
            {
                if (mMemoryBuffer.DatabaseName != value)
                {
                    mMemoryBuffer.DatabaseName = value;
                }
            }
        }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 生成一个Message的Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public long GetId(long id)
        {
            lock (this)
            {
                long ll = id * 10;
                if (mLastId == ll)
                {
                    ll += mCount;
                    mCount++;
                    if(mCount>9)
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    mCount = 0;
                }
                mLastId = ll;
                return ll;
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
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void RaiseMessage(Cdy.Ant.Message msg)
        {
            lock (mMemoryBuffer)
            {
                mMemoryBuffer.AddMessage(msg);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void RestoreMessage(long id,string value)
        {
            var vtime = RestoreTimeFromId(id);
            if (vtime >= mMemoryBuffer.OldestMessageTime)
            {
                mMemoryBuffer.RestoreMessage(id,value);
            }
            else
            {
                HisMessageService.Service.RestoreMessage(id,value);
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
            var vtime = RestoreTimeFromId(id);
            if (vtime >= mMemoryBuffer.OldestMessageTime)
            {
                mMemoryBuffer.AckMessage(id, content, user);
            }
            else
            {
                HisMessageService.Service.AckMessage(id, content, user);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Cdy.Ant.Message Query(long id)
        {
            var vtime = RestoreTimeFromId(id);
            if(vtime>mMemoryBuffer.OldestMessageTime)
            {
                return mMemoryBuffer.Query(id);
            }
            else
            {
                return HisMessageService.Service.Query(id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> Query(DateTime stime, DateTime etime)
        {
            if(etime<mMemoryBuffer.OldestMessageTime)
            {
                return HisMessageService.Service.Query(stime, etime);
            }
            else if(stime>mMemoryBuffer.OldestMessageTime)
            {
                return mMemoryBuffer.Query(stime, etime);
            }
            else
            {
                List<Cdy.Ant.Message> re = new List<Cdy.Ant.Message>();
                re.AddRange(HisMessageService.Service.Query(stime, mMemoryBuffer.OldestMessageTime));
                re.AddRange(mMemoryBuffer.Query(mMemoryBuffer.OldestMessageTime, etime));
                return re;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> Query(DateTime stime, DateTime etime, IEnumerable<QueryFilter> Filters)
        {
            if (etime < mMemoryBuffer.OldestMessageTime)
            {
                return HisMessageService.Service.Query(stime, etime,Filters);
            }
            else if (stime > mMemoryBuffer.OldestMessageTime)
            {
                return mMemoryBuffer.Query(stime, etime, Filters);
            }
            else
            {
                List<Cdy.Ant.Message> re = new List<Cdy.Ant.Message>();
                re.AddRange(HisMessageService.Service.Query(stime, mMemoryBuffer.OldestMessageTime, Filters));
                re.AddRange(mMemoryBuffer.Query(mMemoryBuffer.OldestMessageTime, etime, Filters));
                return re;
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
