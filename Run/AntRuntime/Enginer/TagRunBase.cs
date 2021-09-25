using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TagRunBase
    {

        #region ... Variables  ...
        
        private Tagbase mTag;

        private object mValue;

        private bool mNeedCal;

        private object mLockObj = new object();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public bool NeedCal
        {
            get
            {
                return mNeedCal;
            }
            set
            {
                lock (mLockObj)
                    mNeedCal = value;
            }
        }

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Source { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public virtual Tagbase LinkedTag
        {
            get
            {
                return mTag;
            }
            set
            {
                if (mTag != value)
                {
                    mTag = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual object Value { get { return mValue; } set { mValue = value; } }

        /// <summary>
        /// 
        /// </summary>
        public abstract TagType SupportTag { get; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual List<string> ListLinkTag()
        {
            return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// 执行变量报警
        /// </summary>
        public virtual void LinkExecute()
        {

        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
