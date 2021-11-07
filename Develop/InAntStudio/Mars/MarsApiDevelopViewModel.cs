using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class MarsApiDevelopViewModel:ViewModelBase
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
        public MarsApiData Data { get; set; }

        /// <summary>
            /// 
            /// </summary>
        public string ServerIp
        {
            get
            {
                return Data.ServerIp;
            }
            set
            {
                if (Data.ServerIp != value)
                {
                    Data.ServerIp = value;
                    OnPropertyChanged("ServerIp");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Port
        {
            get
            {
                return Data.Port;
            }
            set
            {
                if (Data.Port != value)
                {
                    Data.Port = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public string UserName
        {
            get
            {
                return Data.UserName;
            }
            set
            {
                if (Data.UserName != value)
                {
                    Data.UserName = value;
                    OnPropertyChanged("UserName");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Password
        {
            get
            {
                return Data.Password;
            }
            set
            {
                if (Data.Password != value)
                {
                    Data.Password = value;
                    OnPropertyChanged("Password");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public int ScanCircle
        {
            get
            {
                return Data.ScanCircle;
            }
            set
            {
                if (Data.ScanCircle != value)
                {
                    Data.ScanCircle = value;
                    OnPropertyChanged("ScanCircle");
                }
            }
        }

        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
