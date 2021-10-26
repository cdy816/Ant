using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Message
{
    /// <summary>
    /// 
    /// </summary>
    public class FileMessageBuffer
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
        public DateTime Starttime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Endtime { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDataFile()
        {
            string name = Starttime.ToString("yyyyMMdd");
            return PathHelper.helper.GetDataPath(name, name + ".alm");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> ReadFromFile()
        {
            string sfile = GetDataFile();
            if(System.IO.File.Exists(sfile))
            {
                using (var vv = System.IO.File.Open(sfile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {

                }
            }
            return null;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
