using Cdy.Ant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class MarsApiData:ApiData
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
        public string ServerIp { get; set; } = "127.0.0.1";

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; } = 14330;

        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; } = "Admin";

        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; } = "Admin";

        /// <summary>
        /// 扫描周期
        /// </summary>
        public int ScanCircle { get; set; } = 1000;

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override XElement SaveToXML()
        {
            var re = base.SaveToXML();
            re.SetAttributeValue("ServerIp", ServerIp);
            re.SetAttributeValue("Port", Port);
            re.SetAttributeValue("UserName", UserName);
            re.SetAttributeValue("Password", Password);
            re.SetAttributeValue("ScanCircle", ScanCircle);
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public override void LoadFromXML(XElement xe)
        {
            if (xe == null) return;
            ServerIp = xe.Attribute("ServerIp") != null ? xe.Attribute("ServerIp").Value : string.Empty;
            UserName = xe.Attribute("UserName") != null ? xe.Attribute("UserName").Value : string.Empty;
            Password = xe.Attribute("Password") != null ? xe.Attribute("Password").Value : string.Empty;
            Port = xe.Attribute("Port") != null ? int.Parse(xe.Attribute("Port").Value) : Port;
            ScanCircle = xe.Attribute("ScanCircle") != null ? int.Parse(xe.Attribute("ScanCircle").Value) : ScanCircle;
            base.LoadFromXML(xe);
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
