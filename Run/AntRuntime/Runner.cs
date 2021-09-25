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
        private APIManager mApi;

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
            LoadApi();
            PathHelper.helper.CheckDataPathExist();
            LoadDatabase();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadDatabase()
        {
            this.mCurrentDatabase = new Cdy.Ant.AlarmDatabaseSerise().Load(Name);
            alarmEnginer = new AlarmEnginer() { Database = mCurrentDatabase };
            alarmEnginer.Init();
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadApi()
        {
            mApi = new APIManager() { Name = Name };
            mApi.Load();
            mApi.Apis.Init();
            ServiceLocator.Locator.Registor<IDataTagApi>(mApi.Apis);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            alarmEnginer.Start();
            mApi.Apis.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            mApi.Apis.Stop();
            alarmEnginer.Stop();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
