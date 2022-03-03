using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntApiAndProxyConfigDevelop
{
    /// <summary>
    /// 
    /// </summary>
    public class GrpcApiDevelop : ViewModelBase, IMessageServiceProxyDevelop
    {

        #region ... Variables  ...

        private int mPort = 15331;
                
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public string TypeName => "GrpcApi";


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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement Save()
        {
            XElement xe = new XElement("GrpcApi");
            xe.SetAttributeValue("Port", Port);
            return xe;
        }
    }
}
