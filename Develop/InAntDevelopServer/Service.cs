﻿using Cdy.Ant;
using DBDevelopService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InAntDevelopServer
{
    public class Service
    {

        #region ... Variables  ...
        
        private GrpcDBService grpcDBService = new GrpcDBService();
        //private WebAPIDBService webDBService = new WebAPIDBService();

        /// <summary>
        /// 
        /// </summary>
        public static Service Instanse = new Service();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        public Service()
        {
            DBDevelopService.SecurityManager.Manager.Init();
                      ////注册日志
            //ServiceLocator.Locator.Registor<ILog>(new ConsoleLogger());
        }

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public void Start(int grpcPort = 5001, int webSocketPort = 8000, bool isEnableGrpc = true, bool isEnableWebApi = true)
        {
            try
            {
             
                LoggerService.Service.Info("Service", "Ready to start....");
                ApiFactory.Factory.LoadForDevelop();
                DbManager.Instance.PartLoad();
                if (isEnableGrpc)
                    grpcDBService.Start(grpcPort);

                //if (isEnableWebApi)
                //    webDBService.Start(webSocketPort);
            }
            catch(Exception ex)
            {
                LoggerService.Service.Erro("Service","start "+ ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            grpcDBService.StopAsync();
            //webDBService.StopAsync();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
