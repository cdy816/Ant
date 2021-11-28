using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntRuntime.Enginer
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TagRunBase:IDisposable
    {

        #region ... Variables  ...
        
        private Tagbase mTag;

        private object mValue;

        protected bool mNeedCal;

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
        public bool IsEnable {
            get 
            { 
                return mTag.IsEnable; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

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
        /// <param name="props"></param>
        public virtual void ModifyProperty(Dictionary<string,string> props)
        {
            foreach(var vv in props)
            {
                switch (vv.Key.ToLower())
                {
                    case "desc":
                        mTag.Desc = vv.Value;
                        break;
                    case "customcontent1":
                        mTag.CustomContent1 = vv.Value;
                        break;
                    case "customcontent2":
                        mTag.CustomContent2 = vv.Value;
                        break;
                    case "customcontent3":
                        mTag.CustomContent3 = vv.Value;
                        break;
                    case "isenable":
                        mTag.IsEnable = bool.Parse(vv.Value);
                        break;
                    default:
                        OnPropertyChangedForRuntime(vv.Key.ToLower(), vv.Value);
                        break;
                }
            }
            OnPropertyChangedFinish();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string,string> GetSupportModifyProperty()
        {
            Dictionary<string, string> re = new Dictionary<string, string>();
            re.Add("Desc", mTag.Desc);
            re.Add("CustomContent1", mTag.CustomContent1);
            re.Add("CustomContent2", mTag.CustomContent2);
            re.Add("CustomContent3", mTag.CustomContent3);
            re.Add("IsEnable", mTag.IsEnable.ToString());
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected virtual void OnPropertyChangedForRuntime(string name,string value)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnPropertyChangedFinish()
        {

        }

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
        /// 
        /// </summary>
        public virtual void PreRun()
        {

        }

        /// <summary>
        /// 执行变量报警
        /// </summary>
        public virtual void LinkExecute()
        {

        }

        /// <summary>
        /// 保存运行时状态
        /// </summary>
        /// <param name="keyName">关键列</param>
        /// <returns></returns>
        public virtual XElement SaveRuntimeStatue(string keyName)
        {
            XElement xe = new XElement(this.GetType().Name);
            xe.SetAttributeValue("Id", keyName);
            return xe;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void LoadRuntimeStatue(XElement xe)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
