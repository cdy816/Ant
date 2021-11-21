using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cdy.Ant.Tag
{

    /// <summary>
    /// 过滤操作类型，0:大于,1:小于,2:等于,3:字符串包含
    /// </summary>
    public enum FilterOperate
    {
        /// <summary>
        /// 大于
        /// </summary>
        Great,
        /// <summary>
        /// 小于
        /// </summary>
        Low,
        /// <summary>
        /// 等于
        /// </summary>
        Equals,
        /// <summary>
        /// 包含
        /// </summary>
        Contains
    }

    /// <summary>
    /// 过滤条件
    /// </summary>
    public struct QueryFilter
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 类型，0:大于,1:小于,2:等于,3:字符串包含
        /// </summary>
        public FilterOperate Opetate { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public interface IMessageQuery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Message Query(long id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        void AckMessage(long id,string content,string username);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lid"></param>
        /// <param name="content"></param>
        /// <param name="username"></param>
        void DeleteMessage(long lid, string content, string username);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        IEnumerable<Cdy.Ant.Message> Query(DateTime stime, DateTime etime);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>

        IEnumerable<Cdy.Ant.Message> Query(DateTime stime, DateTime etime,IEnumerable<QueryFilter> Filters);

    }
}
