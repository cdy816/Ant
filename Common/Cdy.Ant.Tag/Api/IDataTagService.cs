using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDataTagService
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 获取变量值
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        object GetTagValue(string tagName);

        /// <summary>
        /// 设置变量值
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool SetTagValue(string tagname, object value);

        /// <summary>
        /// 获取当前值的质量戳
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        int GetTagValueQuality(string tagName);

        /// <summary>
        /// 注册要监视的变量
        /// </summary>
        /// <param name="tag"></param>
        void RegistorMonitorTag(IEnumerable<string> tag);

        /// <summary>
        /// 变量值改变时，回调
        /// </summary>
        /// <param name="callback"></param>
        void RegistorTagChangeCallBack(Action<string, object> callback);


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
