using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataTagApi
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        abstract string TypeName { get; }

        /// <summary>
        /// 
        /// </summary>
        IDataTagService TagService { get; }

        #endregion ...Properties...

        #region ... Methods    ...

        void Init();

        /// <summary>
        /// 
        /// </summary>
        void Start();


        /// <summary>
        /// 
        /// </summary>
        void Stop();

        /// <summary>
        /// 
        /// </summary>
        void Load(XElement xe);
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    public interface IApiForFactory
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
        string TypeName { get; }
        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        void Load(XElement xe);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IDataTagApi NewApi();

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
