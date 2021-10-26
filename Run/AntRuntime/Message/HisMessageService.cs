using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Message
{
    /// <summary>
    /// 历史报警查询
    /// </summary>
    public class HisMessageService: IMessageQuery
    {

        #region ... Variables  ...
        /// <summary>
        /// 
        /// </summary>
        public static HisMessageService Service = new HisMessageService();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lid"></param>
        public Cdy.Ant.Message Query(long lid)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> Query(DateTime stime,DateTime etime, IEnumerable<QueryFilter> Filters)
        {
            var re = Query(stime, etime);
           
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> Query(DateTime stime, DateTime etime)
        {
            List<FileMessageBuffer> mReaders = new List<FileMessageBuffer>();
            DateTime st = new DateTime(stime.Year, stime.Month, stime.Day, stime.Hour, 0, 0);
            DateTime dt = DateTime.MinValue;
            do
            {
                dt = st.AddHours(1);
                if (dt.Day > st.Day)
                {
                    mReaders.Add(new FileMessageBuffer() { Starttime = st, Endtime = dt });
                    st = dt;
                }
                else if(dt>etime)
                {
                    mReaders.Add(new FileMessageBuffer() { Starttime = st, Endtime = dt });
                }
            }
            while (dt < etime);

            List<Cdy.Ant.Message> re = new List<Cdy.Ant.Message>();
            foreach (var vv in mReaders)
            {
                var vtmp = vv.ReadFromFile();
                if (vtmp != null)
                {
                    re.AddRange(vtmp);
                }
            }

            return re;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
