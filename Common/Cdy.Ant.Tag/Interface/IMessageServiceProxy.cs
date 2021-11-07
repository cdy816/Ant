using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cdy.Ant.Tag
{
    public interface IMessageServiceProxy
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
        /// <param name="messageQuery"></param>
        void Init(IMessageQuery messageQuery);

        /// <summary>
        /// 
        /// </summary>
        void Start();

        /// <summary>
        /// 
        /// </summary>
        void Stop();

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IMessageServiceProxyDevelop
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
        /// 开始时配置
        /// </summary>
        object Config();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        void Load(XElement xe);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        XElement Save();


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
