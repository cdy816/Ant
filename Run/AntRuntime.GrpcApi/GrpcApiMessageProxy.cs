using Cdy.Ant.Tag;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
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

        WebApplication app;

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
            Task.Run(() =>
            {
                //var builder = new WebApplicationBuilder();
                var builder = WebApplication.CreateBuilder();

                if (IsWin7)
                {
                    //Win 7 的情况下使用 不支持TLS 的 HTTP/2
                    builder.WebHost.ConfigureKestrel(options =>
                    {
                        options.Listen(System.Net.IPAddress.Parse("0.0.0.0"), Port, a => a.Protocols =
                             Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
                    });
                }
                else
                {
                    string spath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mars.pfx");
                    if (System.IO.File.Exists(spath))
                    {
                        builder.WebHost.ConfigureKestrel(options =>
                        {
                            options.ListenAnyIP(Port, listenOps =>
                            {
                                listenOps.UseHttps(callback =>
                                {
                                    callback.AllowAnyClientCertificate();
                                    callback.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(spath, "mars");
                                });
                            });
                        });
                    }
                    else
                    {
                        builder.WebHost.UseUrls("https://0.0.0.0:" + Port);
                    }
                }
                //if (IsWin7)
                //{
                //    builder.WebHost.UseUrls("http://0.0.0.0:" + Port);
                //}
                //else
                //{
                //    builder.WebHost.UseUrls("https://0.0.0.0:" + Port);
                //}
                builder.Services.AddGrpc();
                app = builder.Build();
                app.MapGrpcService<Services.MessageService>();
                app.MapGet("/", () => "Message Grpc Service.");
                app.Run();
            });

            MessageHelper.Helper.Start();
        }

      

        /// <summary>
        /// 
        /// </summary>
        public static bool IsWin7
        {
            get
            {
                return Environment.OSVersion.Version.Major < 8 && Environment.OSVersion.Platform == PlatformID.Win32NT;
            }
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
            if(app != null)
            {
                app.StopAsync();
            }

            MessageHelper.Helper.Stop();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }
}
