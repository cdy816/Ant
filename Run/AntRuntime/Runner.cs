using AntRuntime.Enginer;
using Cdy.Ant;
using System;

namespace AntRuntime
{
    /// <summary>
    /// 
    /// </summary>
    public class Runner
    {

        #region ... Variables  ...

        private Cdy.Ant.AlarmDatabase mCurrentDatabase;
        private AlarmEnginer alarmEnginer;
        private IDataTagApi mDataApi;
        private Cdy.Ant.Tag.IMessageServiceProxy mServiceProxy;

        private SecurityRunner mSecurityRunner;

        //private APIManager mApi;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        static Runner()
        {
            ServiceLocator.Locator.Registor<ILog>(new ConsoleLogger());
            ApiFactory.Factory.LoadForRun();
            ServiceLocator.Locator.Registor<IApiFactory>(ApiFactory.Factory);
            ServiceLocator.Locator.Registor<Cdy.Ant.Tag.IMessageQuery>(MessageService.Service);
        }

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsStarted { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckNameExit(string name)
        {
            return System.IO.File.Exists(PathHelper.helper.GetDataPath(name, name + ".adb"));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            PathHelper.helper.CheckDataPathExist();
           
            LoadDatabase();
            LoadServerProxy();
            LoadApi();
            alarmEnginer.Init();

            MessageService.Service.LoadLastBuffer();

            alarmEnginer.LoadTagStatus();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadDatabase()
        {
            this.mCurrentDatabase = new Cdy.Ant.AlarmDatabaseSerise().LoadByName(Name);
            alarmEnginer = new AlarmEnginer() { Database = mCurrentDatabase };
          
            HisMessageService.Service.DatabaseName = mCurrentDatabase.Name;
            MessageService.Service.DatabaseName = mCurrentDatabase.Name;

            mSecurityRunner = new SecurityRunner() { Document = new SecuritySerise().LoadByName(mCurrentDatabase.Name) };
            ServiceLocator.Locator.Registor<Cdy.Ant.Tag.IRuntimeSecurity>(mSecurityRunner);

            ServiceLocator.Locator.Registor<Cdy.Ant.Tag.IRuntimeTagService>(alarmEnginer);
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadApi()
        {
            if (!string.IsNullOrEmpty(mCurrentDatabase.Setting.ApiType))
            {
                var api = ApiFactory.Factory.GetRuntimeInstance(mCurrentDatabase.Setting.ApiType);
                api.Load(mCurrentDatabase.Setting.ApiData);
                api.Init();
                ServiceLocator.Locator.Registor<IDataTagApi>(api);
                mDataApi = api;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadServerProxy()
        {
            if(!string.IsNullOrEmpty(mCurrentDatabase.Setting.ProxyType))
            {
                var ss = ProxyServiceFactory.Factory.LoadForRun(mCurrentDatabase.Setting.ProxyType);
                ss.Load(mCurrentDatabase.Setting.ProxyData);
                ss.Init(ServiceLocator.Locator.Resolve<Cdy.Ant.Tag.IMessageQuery>());
                mServiceProxy = ss;
                ServiceLocator.Locator.Registor<Cdy.Ant.Tag.IMessageServiceProxy>(ss);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            LoggerService.Service.Info("Runner", "Start the " + this.Name + " ... ");

            alarmEnginer.Start();
          
            mSecurityRunner?.Start();
            HisMessageService.Service.Start();
            mDataApi?.Start();
            mServiceProxy?.Start();
            IsStarted = true;

            LoggerService.Service.Info("Runner", "Start the " + this.Name + " complete.");
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReStartDatabase()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            LoggerService.Service.Info("Runner", "Stop the " + this.Name + " ... ");

            mDataApi?.Stop();
          
            alarmEnginer.Stop();
            mSecurityRunner?.Stop();
            
            HisMessageService.Service.Stop();
            MessageService.Service.FlushDirtyBufferToDisk();

            mServiceProxy?.Stop();

            alarmEnginer.SaveTagStatus();

            IsStarted = false;

            LoggerService.Service.Info("Runner", "Stop the " + this.Name + " complete.");
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
