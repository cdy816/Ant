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
        /// 查询变量的历史值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="span">时间间隔</param>
        /// <returns></returns>
        Dictionary<DateTime, Tuple<object, byte>> QueryHisValue(string tagName, DateTime startTime, DateTime endTime, TimeSpan span);

        /// <summary>
        /// 查询某个时间段记录的所有的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        Dictionary<DateTime, Tuple<object, byte>> QueryAllHisValue(string tagName, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 查询执行时刻点的的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="times">时间列表</param>
        /// <returns></returns>
        Dictionary<DateTime, Tuple<object, byte>> QueryHisValue(string tagName, List<DateTime> times);


        /// <summary>
        /// 查询变量好的历史值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="span">时间间隔</param>
        /// <returns></returns>
        Dictionary<DateTime, object> QueryGoodHisValue(string tagName, DateTime startTime, DateTime endTime,TimeSpan span);

        /// <summary>
        /// 查询某个时间段记录的所有好的值
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<DateTime, object> QueryAllGoodHisValue(string tagName, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 查询执行时刻点的好的值
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        Dictionary<DateTime, object> QueryGoodHisValue(string tagName, List<DateTime> times);

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

        /// <summary>
        /// 查找大于指定值得值和时间
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<DateTime, object> FindNumberTagValuesGreaterThan(string tagname, object value, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 查找小于指定值得值和时间
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<DateTime, object> FindNumberTagValuesLessThan(string tagname, object value, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 查找等于指定值得值和时间
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <param name="interval">偏差</param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<DateTime, object> FindNumberTagValuesEquals(string tagname, object value,double interval, DateTime startTime, DateTime endTime);



        /// <summary>
        /// 查找枚举所有最大值
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<DateTime,object> FindNumberTagMaxValues(string tagname,  DateTime startTime, DateTime endTime);


        /// <summary>
        /// 查找所有最小值
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<DateTime, object> FindNumberTagMinValues(string tagname, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 计算平均值
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        double CalNumberTagAvgValue(string tagname, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 计算指定值保持时间
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <param name="interval"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        double CalNumberTagValueKeepTime(string tagname, object value, double interval, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 计算大于指定值保持时间
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <param name="interval"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        double CalNumberTagGreateThanValueKeepTime(string tagname, object value, double interval, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 计算小于指定值保持时间
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <param name="interval"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        double CalNumberTagLessThanValueKeepTime(string tagname, object value, double interval, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 查找非数值型变量，等于指定值的时刻
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        List<DateTime> FindNoNumberTagValues(string tagname,DateTime startTime, DateTime endTime, object value);


        /// <summary>
        /// 计算非数值型变量等于指定值保持时间
        /// </summary>
        /// <param name="tagname"></param>
        /// <param name="value"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        double CalNoNumberTagValueKeepTime(string tagname, object value, DateTime startTime, DateTime endTime);

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
