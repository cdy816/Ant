using InAntStudio;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DbManager.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 
        /// </summary>
        public App()
        {
            this.Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                AutoLoginHelper.Helper.Server = e.Args[0];
                AutoLoginHelper.Helper.UserName = e.Args.Length > 1 ? e.Args[1] : string.Empty;
                AutoLoginHelper.Helper.Password = e.Args.Length > 2 ? e.Args[2] : string.Empty;
                AutoLoginHelper.Helper.Database = e.Args.Length > 3 ? e.Args[3] : string.Empty;
            }
        }
    }
}
