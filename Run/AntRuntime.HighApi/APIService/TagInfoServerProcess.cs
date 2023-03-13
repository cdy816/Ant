//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/5/14 11:00:38.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cheetah;
using System.Linq;
using System.Xml.Linq;
using Cdy.Ant.Tag;
using Cdy.Ant;
//using DotNetty.Buffers;

namespace AntRuntime.HighApi
{
    public class TagInfoServerProcess : ServerProcessBase
    {

        #region ... Variables  ...
        
        //
        public const byte GetTagIdByNameFun = 0;

        //
        public const byte ListTags = 2;

        public const byte ModifyTag = 3;

        public const byte QueryTagProperty = 4;

        public const byte Login = 1;

        public const byte Heart = 5;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        public override byte FunId => ApiFunConst.TagInfoRequest;
                
        #endregion ...Properties...

        #region ... Methods    ...

       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        protected unsafe override void ProcessSingleData(string client, ByteBuffer data)
        {
            byte sfun = data.ReadByte();
            switch (sfun)
            {
                case ListTags:
                    long loginId = data.ReadLong();
                    if (ServiceLocator.Locator.Resolve<IRuntimeSecurity>().CheckLogin(loginId))
                    {
                        var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ListTagNames();
                        StringBuilder sb = new StringBuilder();
                        foreach(var vv in re)
                        {
                            sb.Append(vv.ToString()+",");
                        }
                        sb.Length = sb.Length>0?sb.Length-1:sb.Length;
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal))
                            {
                                var aa = Parent.Allocate(ApiFunConst.TagInfoRequest, (int)ms.Position + 5);
                                aa.Write((byte)ListTags);
                                aa.Write((int)ms.Position);
                                aa.Write(ms.GetBuffer(), 0, (int)ms.Position);
                                Parent.AsyncCallback(client, aa);
                            }
                        }

                    }
                    break;
                case ModifyTag:
                    loginId = data.ReadLong();
                    if (ServiceLocator.Locator.Resolve<IRuntimeSecurity>().CheckLogin(loginId))
                    {
                        string tag = data.ReadString();
                        string props = data.ReadString();
                        var vtags = props.Split(";");
                        Dictionary<string, string> prps = new Dictionary<string, string>();
                        foreach(var vv in vtags)
                        {
                            var vvv = vv.Split(",");
                            prps.Add(vvv[0], vvv[1]);

                        }
                        var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ModifyTag(tag, prps);

                        Parent.AsyncCallback(client, ToByteBuffer(ApiFunConst.TagInfoRequest, re?(byte)1: (byte)0));
                    }
                    break;
                case QueryTagProperty:
                    loginId = data.ReadLong();
                    if (ServiceLocator.Locator.Resolve<IRuntimeSecurity>().CheckLogin(loginId))
                    {
                        var vname = data.ReadString();

                        var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ListTagDefines(vname);
                        StringBuilder sb = new StringBuilder();
                        foreach (var vv in re)
                        {
                            sb.Append(vv.Key+","+ vv.ToString() + ";");
                        }
                        sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal))
                            {
                                var aa = Parent.Allocate(ApiFunConst.TagInfoRequest, (int)ms.Position + 5);
                                aa.Write((byte)QueryTagProperty);
                                aa.Write((int)ms.Position);
                                aa.Write(ms.GetBuffer(), 0, (int)ms.Position);
                                Parent.AsyncCallback(client, aa);
                            }
                        }
                    }
                    break;
               
                case Heart:
                    loginId = data.ReadLong();
                    ServiceLocator.Locator.Resolve<IRuntimeSecurity>().FreshUserId(loginId.ToString());
                    break;
                case Login:
                    string user = data.ReadString();
                    string pass = data.ReadString();
                    long result = ServiceLocator.Locator.Resolve<IRuntimeSecurity>().Login(user, pass, client);
                    Parent.AsyncCallback(client, ToByteBuffer(ApiFunConst.TagInfoRequest, result));
                    LoggerService.Service.Info("TagInfo", $"客户端 {client} 的用户 {user} 登录成功!");
                    break;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public override void OnClientDisconnected(string id)
        {
            base.OnClientDisconnected(id);
            try
            {
                var vss = id.Split(":");
                //Cdy.Tag.Common.ProcessMemoryInfo.Instances.RemoveClient(vss[0], int.Parse(vss[1]));
            }
            catch
            {

            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }
}
