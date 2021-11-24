using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdy.Ant.Tag
{
    /// <summary>
    /// 
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// 用于缓存中间数据的对象
        /// </summary>
        public static Dictionary<string, object> GlobalObject { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 获取变量的值
        /// </summary>
        /// <param name="tag">格式:"组名.点名"</param>
        /// <returns></returns>
        public static object GetTagValue(string tag)
        {
            return null;
        }

        /// <summary>
        /// 设置变量的值
        /// </summary>
        /// <param name="tag">格式:"组名.点名"</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetTagValue(string tag,object value)
        {
            return true;
        }

        /// <summary>
        /// 变量的质量戳是否为有效值
        /// </summary>
        /// <param name="tag">格式:"组名.点名"</param>
        /// <returns></returns>
        public static bool IsTagQualityGood(string tag)
        {
            return true;
        }


        /// <summary>
        /// 对变量的值求和
        /// </summary>
        /// <param name="tags">变量名称集合 变量名格式:"组名.点名"</param>
        /// <returns></returns>
        public static double TagValueSum(params string[] tags)
        {
            return 0;
        }

        /// <summary>
        /// 对变量的值求平均值
        /// </summary>
        /// <param name="tags">变量名称集合 变量名格式:"组名.点名"</param>
        /// <returns></returns>
        public static double TagValueAvg(params string[] tags)
        {
            return 0;
        }

        /// <summary>
        /// 对变量的值取最大
        /// </summary>
        /// <param name="tags">变量名称集合 变量名格式:"组名.点名"</param>
        /// <returns></returns>
        public static double TagValueMax(params string[] tags)
        {
            return 0;
        }

        /// <summary>
        /// 对变量的值取最小
        /// </summary>
        /// <param name="tags">变量名称集合 变量名格式:"组名.点名"</param>
        /// <returns></returns>
        public static double TagValueMin(params string[] tags)
        {
            return 0;
        }

        /// <summary>
        /// 对一组数值进行求平均值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Avg(params object[] values)
        {
            return 0;
        }

        /// <summary>
        /// 对一组数值取最大值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Max(params object[] values)
        {
            return 0;
        }

        /// <summary>
        /// 对一组数值取最小值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Min(params object[] values)
        {
            return 0;
        }

        /// <summary>
        /// 对值进行取位
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="index">要取位的序号，从0开始</param>
        /// <returns></returns>
        public static byte Bit(object value,byte index)
        {
            return 0;
        }

        /// <summary>
        /// 查询变量的历史值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="span">时间间隔</param>
        /// <returns></returns>
        public static Dictionary<DateTime,Tuple<object,byte>> QueryHisValue(string tagName,DateTime startTime,DateTime endTime,TimeSpan span)
        {
            return null;
        }

        /// <summary>
        /// 查询某个时间段记录的所有的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static Dictionary<DateTime, Tuple<object, byte>> QueryAllHisValue(string tagName, DateTime startTime, DateTime endTime)
        {
            return null;
        }

        /// <summary>
        /// 查询执行时刻点的的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="times">时间点集合</param>
        /// <returns></returns>
        public static Dictionary<DateTime, Tuple<object, byte>> QueryHisValue(string tagName, List<DateTime> times)
        {
            return null;
        }


        /// <summary>
        /// 查询变量好的历史值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="span">时间间隔</param>
        /// <returns></returns>
        public static Dictionary<DateTime, object> QueryGoodHisValue(string tagName, DateTime startTime, DateTime endTime, TimeSpan span)
        {
            
            return null;
        }

        /// <summary>
        /// 查询某个时间段记录的所有好的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static Dictionary<DateTime, object> QueryAllGoodHisValue(string tagName, DateTime startTime, DateTime endTime)
        {
            return null;
        }

        /// <summary>
        /// 查询执行时刻点的好的值
        /// </summary>
        /// <param name="tagName">变量名称</param>
        /// <param name="times">时间点集合</param>
        /// <returns></returns>
        public static Dictionary<DateTime, object> QueryGoodHisValue(string tagName, List<DateTime> times)
        {
            return null;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Message
    {

        /// <summary>
        /// 查询消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static IEnumerable<Cdy.Ant.Message> QueryMessage(DateTime startTime, DateTime endTime)
        {
            return null;
        }

        /// <summary>
        /// 查询报警消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static IEnumerable<Cdy.Ant.AlarmMessage> QueryAlarmMessage(DateTime startTime, DateTime endTime)
        {
            return null;
        }

        /// <summary>
        /// 查询消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="filters">过滤条件</param>
        /// <returns></returns>
        public static IEnumerable<Cdy.Ant.Message> QueryMessage(DateTime startTime, DateTime endTime, params string[] filters)
        {
            return null;
        }

        /// <summary>
        /// 查询报警消息
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="filters">过滤条件</param>
        /// <returns></returns>
        public static IEnumerable<Cdy.Ant.AlarmMessage> QueryAlarmMessage(DateTime startTime, DateTime endTime, params string[] filters)
        {
            return null;
        }


        /// <summary>
        /// 产生报警
        /// </summary>
        /// <param name="messageBody">消息体</param>
        /// <param name="value">报警值</param>
        /// <param name="level">报警级别</param>
        public static void Alarm(string messageBody,string value,Cdy.Ant.AlarmLevel level)
        {
            
        }

        /// <summary>
        /// 产生报警
        /// </summary>
        /// <param name="value">报警值</param>
        /// <param name="level">报警级别</param>
        public static void Alarm(string value,Cdy.Ant.AlarmLevel level)
        {

        }

        /// <summary>
        /// 产生一条消息
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {

        }

        /// <summary>
        /// 恢复报警
        /// </summary>
        /// <param name="value">恢复值</param>
        public static void Restore(string value)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// 输出提示信息
        /// </summary>
        /// <param name="source">触发者</param>
        /// <param name="msg">消息内容</param>
        public static void Info(string source,string msg)
        {

        }

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="source">触发者</param>
        /// <param name="msg">消息内容</param>
        public static void Warn(string source, string msg)
        {

        }

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="source">触发者</param>
        /// <param name="msg">消息内容</param>
        public static void Erro(string source, string msg)
        {

        }
    }
}
