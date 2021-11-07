using Cdy.Ant.Tag;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AntRutime.WebApi
{
    public class WebApiMessageProxy : IMessageServiceProxy
    {

        #region ... Variables  ...
        public static  IMessageQuery MessageService;
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
        public static bool UseHttps { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TypeName => "WebApi";

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   if (UseHttps)
                   {
                       webBuilder.UseUrls("https://0.0.0.0:" + Port);
                   }
                   else
                   {
                       webBuilder.UseUrls("http://0.0.0.0:" + Port);
                   }
                   webBuilder.UseStartup<Startup>();
               });

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
            CreateHostBuilder(null).Build().Run();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMessageServiceProxy NewApi()
        {
            return new WebApiMessageProxy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public void Load(XElement xe)
        {
            if (xe.Attribute("UseHttps") != null)
            {
                UseHttps = bool.Parse(xe.Attribute("UseHttps").Value);
            }
            Port = int.Parse(xe.Attribute("ServerPort")?.Value);
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
