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
    public enum FilterOperate
    {
        Great,
        Low,
        Equals,
        Contains
    }

    public struct QueryFilter
    {
        /// <summary>
        /// 
        /// </summary>
        public string PropertyName { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public FilterOperate Opetate { get; set; }

        /// <summary>
        /// 
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
