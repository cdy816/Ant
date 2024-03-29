﻿using System;
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

        private string GetOpeString(FilterOperate ope)
        {
            switch (ope)
            {
                case FilterOperate.Equals:
                    return "==";
                case FilterOperate.Contains:
                    return "..";
                case FilterOperate.Great:
                    return ">";
                case FilterOperate.Low:
                    return "<";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return PropertyName + GetOpeString(Opetate) + Value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class FilterExtends
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sval"></param>
        /// <returns></returns>
        public static QueryFilter GetFilterFromString(this string sval)
        {
            QueryFilter re = new QueryFilter();
            if (sval.Contains("=="))
            {
                string[] ss = sval.Split("==");
                re = new QueryFilter() { PropertyName = ss[0], Value = ss[2], Opetate = FilterOperate.Equals };
            }
            else if (sval.Contains(">"))
            {
                string[] ss = sval.Split(">");
                re = new QueryFilter() { PropertyName = ss[0], Value = ss[2], Opetate = FilterOperate.Great };
            }
            else if (sval.Contains("<"))
            {
                string[] ss = sval.Split("<");
                re = new QueryFilter() { PropertyName = ss[0], Value = ss[2], Opetate = FilterOperate.Low };
            }
            else if (sval.Contains(".."))
            {
                string[] ss = sval.Split("..");
                re = new QueryFilter() { PropertyName = ss[0], Value = ss[2], Opetate = FilterOperate.Contains };
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sval"></param>
        /// <returns></returns>
        public static IEnumerable<QueryFilter> GetFiltersFromString(this IEnumerable<string> sval)
        {
            List<QueryFilter> re = new List<QueryFilter>();
            if (sval == null) return re;
            foreach (var vv in sval)
            {
                re.Add(vv.GetFilterFromString());
            }
            return re;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public interface IMessageQuery
    {
        public delegate void NewMessageDelegate(Cdy.Ant.Tag.Message msg);

        public event NewMessageDelegate NewMessage;

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
        IEnumerable<Cdy.Ant.Tag.Message> Query(DateTime stime, DateTime etime);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>

        IEnumerable<Cdy.Ant.Tag.Message> Query(DateTime stime, DateTime etime,IEnumerable<QueryFilter> Filters);

    }
}
