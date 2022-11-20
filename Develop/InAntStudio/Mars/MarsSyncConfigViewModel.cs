using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class MarsSyncConfigViewModel : WindowViewModelBase
    {

        #region ... Variables  ...

        private static string mServerAddress = "127.0.0.1:9000";

        private static string mServerPassword = "Admin";

        private static string mServerUserName = "Admin";

        private string mCurrentDatabase = "";

        DBDevelopClientWebApi.DevelopServiceHelper mHelper;

        private List<string> mDatabases;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        public MarsSyncConfigViewModel()
        {
            LoadConfig();
        }

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
            /// 
            /// </summary>
        public List<string> Databases
        {
            get
            {
                return mDatabases;
            }
            set
            {
                if (mDatabases != value)
                {
                    mDatabases = value;
                    OnPropertyChanged("Databases");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string CurrentDatabase
        {
            get
            {
                return mCurrentDatabase;
            }
            set
            {
                if (mCurrentDatabase != value)
                {
                    mCurrentDatabase = value;
                    OnPropertyChanged("CurrentDatabase");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string ServerPassword
        {
            get
            {
                return mServerPassword;
            }
            set
            {
                if (mServerPassword != value)
                {
                    mServerPassword = value;
                    OnPropertyChanged("ServerPassword");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string ServerAddress
        {
            get
            {
                return mServerAddress;
            }
            set
            {
                if (mServerAddress != value)
                {
                    mServerAddress = value;
                    if (!value.Contains(":"))
                    {
                        mServerAddress = value + ":9000";
                    }
                    OnPropertyChanged("ServerAddress");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ServerUserName
        {
            get
            {
                return mServerUserName;
            }
            set
            {
                if (mServerUserName != value)
                {
                    mServerUserName = value;
                    OnPropertyChanged("ServerUserName");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PreLoadDatabase { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            try
            {
                mHelper = new DBDevelopClientWebApi.DevelopServiceHelper();
                mHelper.Server = ServerAddress;
                if (!mHelper.Server.StartsWith("http://"))
                {
                    mHelper.Server = "http://" + mHelper.Server;
                }
                if (mHelper.Login(ServerUserName, ServerPassword))
                {
                    Databases = mHelper.QueryDatabase().Select(e => e.Name).ToList();

                    if (Databases.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(PreLoadDatabase) && Databases.Contains(PreLoadDatabase))
                        {
                            CurrentDatabase = PreLoadDatabase;
                        }
                        else
                        {
                            CurrentDatabase = Databases[0];
                        }
                    }
                }
                else
                {
                    Databases = new List<string>();
                    CommandManager.InvalidateRequerySuggested();
                    MessageBox.Show("Logging server failed!");
                }
                OnPropertyChanged("ConnectString");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadConfig()
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "MarsApiConfig.cach");
            if (System.IO.File.Exists(sfile))
            {
                XElement xx = XElement.Load(sfile);
                this.ServerAddress = xx.Attribute("ServerAddress")?.Value;
                this.ServerUserName = xx.Attribute("ServerUserName")?.Value;
                if(xx.Attribute("ServerPassword") !=null)
                this.ServerPassword = xx.Attribute("ServerPassword")?.Value;
            }
        }



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
