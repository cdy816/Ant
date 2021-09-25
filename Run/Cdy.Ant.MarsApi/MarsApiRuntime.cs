using System;
using System.Xml.Linq;

namespace Cdy.Ant.MarsApi
{
    /// <summary>
    /// 
    /// </summary>
    public class MarsApiRuntime : ApiBase
    {

        #region ... Variables  ...
        private ApiRunner mRunner;
        private MarsApiData mData;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        public MarsApiRuntime()
        {
            mRunner = new ApiRunner();
            mTagService = mRunner;
        }
        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public override string TypeName => "MarsApi";

        /// <summary>
        /// 
        /// </summary>
        public override ApiData Data => mData;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IDataTagApi NewApi()
        {
            return new MarsApiRuntime() { mData = new MarsApiData() };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void Load(XElement xe)
        {
            mData = new MarsApiData();
            mData.LoadFromXML(xe);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            base.Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {
            
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...








    }
}
