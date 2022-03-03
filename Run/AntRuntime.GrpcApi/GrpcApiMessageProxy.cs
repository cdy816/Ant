using Cdy.Ant.Tag;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntRuntime.GrpcApi
{
    /// <summary>
    /// 
    /// </summary>
    public class GrpcApiMessageProxy : IMessageServiceProxy
    {

        #region ... Variables  ...
        /// <summary>
        /// 
        /// </summary>
        public static  IMessageQuery MessageService=null;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public static int Port { get; set; } = 15331;


        /// <summary>
        /// 
        /// </summary>
        public string TypeName => "GrpcApi";

        #endregion ...Properties...

        #region ... Methods    ...


        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageQuery"></param>
        public void Init(IMessageQuery messageQuery)
        {
            MessageService = messageQuery;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Task.Run(() => {
                var builder = WebApplication.CreateBuilder();

                builder.Services.AddGrpc();
                var app = builder.Build();
                app.MapGrpcService<Services.MessageService>();
                app.MapGet("/", () => "Message Grpc Service.");
                app.Run();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMessageServiceProxy NewApi()
        {
            return new GrpcApiMessageProxy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public void Load(XElement xe)
        {
            Port = int.Parse(xe.Attribute("Port").Value);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            //
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }
}
