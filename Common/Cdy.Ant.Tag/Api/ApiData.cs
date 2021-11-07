using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cdy.Ant
{
    public class ApiData
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
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public virtual void LoadFromXML(XElement xe)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual XElement SaveToXML()
        {
            var re = new XElement("AntApi");
            return re;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
