using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageService
    {

        #region ... Variables  ...
        
        /// <summary>
        /// 
        /// </summary>
        public static MessageService Service = new MessageService();

        private long mLastId = 0;
        private int mCount = 0;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

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
        /// <param name="msg"></param>
        public void RaiseMessage(Cdy.Ant.Message msg)
        {
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Cdy.Ant.Message GetMessage(long id)
        {
            return null;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
