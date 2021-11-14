using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntApiAndProxyConfigDevelop
{
    public class WebApiDevelop :ViewModelBase, IMessageServiceProxyDevelop
    {

        #region ... Variables  ...

        private int mPort = 15331;
        
        private bool mEnableHttps = false;
        
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public string TypeName => "WebApi";


        /// <summary>
            /// 
            /// </summary>
        public int Port
        {
            get
            {
                return mPort;
            }
            set
            {
                if (mPort != value)
                {
                    mPort = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public bool EnableHttps
        {
            get
            {
                return mEnableHttps;
            }
            set
            {
                if (mEnableHttps != value)
                {
                    mEnableHttps = value;
                    OnPropertyChanged("EnableHttps");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Config()
        {
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public void Load(XElement xe)
        {
            if (xe == null) return;
            if(xe.Attribute("Port")!=null)
            {
                Port = int.Parse(xe.Attribute("Port").Value);
            }

            if (xe.Attribute("UseHttps") != null)
            {
                EnableHttps = bool.Parse(xe.Attribute("UseHttps").Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement Save()
        {
            XElement xe = new XElement("WebApi");
            xe.SetAttributeValue("Port", Port);
            xe.SetAttributeValue("UseHttps", EnableHttps);
            return xe;
        }
    }
}
