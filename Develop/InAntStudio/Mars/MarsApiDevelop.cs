using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class MarsApiDevelop : Cdy.Ant.ApiDevelopBase
    {

        #region ... Variables  ...
        private MarsApiData mData = new MarsApiData();
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override ApiData Data { get => mData; set => mData=value as MarsApiData; }

        /// <summary>
        /// 
        /// </summary>
        public override string TypeName => "MarsApi";


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ConfigTags()
        {
            MarsTagBrowserViewModel mbb = new MarsTagBrowserViewModel();
            if(mbb.ShowDialog().Value)
            {
                return mbb.SelectTagName;
            }
            return base.ConfigTags();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object Config()
        {
            return new MarsApiDevelopViewModel() { Data = mData };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ApiData CreatNewData()
        {
            return new MarsApiData();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IDataTagApiDevelop NewApi()
        {
            return new MarsApiDevelop();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...



    }
}
