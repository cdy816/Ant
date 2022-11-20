//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/8/17 10:52:11.
//  Version 1.0
//  种道洋
//==============================================================

using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public class ProxyServiceFactory: IProxyServiceFactory
    {

        #region ... Variables  ...

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, IMessageServiceProxy> mRuntimeManagers = new Dictionary<string, IMessageServiceProxy>();

        /// <summary>
        /// 
        /// </summary>
        private List<string> mProxyService = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public static ProxyServiceFactory Factory = new ProxyServiceFactory();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        public ProxyServiceFactory()
        {
            Init();
        }
        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public List<string> ProxyService
        {
            get
            {
                return mProxyService;
            }
        }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IMessageServiceProxyDevelop GetDevelopInstance(string type)
        {
            return LoadForDevelop(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMessageServiceProxyDevelop GetDevelopInstance()
        {
            return GetDevelopInstance(mProxyService.First());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IMessageServiceProxy GetRuntimeInstance(string type)
        {
            return LoadForRun(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string GetAssemblyPath(string file)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), file);
        }

        /// <summary>
        /// 
        /// </summary>
        public IMessageServiceProxy LoadForRun(string sname)
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "Config", "MessageServerRuntime.cfg");
            if(System.IO.File.Exists(sfile))
            {
                XElement xx = XElement.Load(sfile);
                foreach (var vv in xx.Elements())
                {
                    string ass = vv.Attribute("Assembly").Value;
                    string cls = vv.Attribute("Class").Value;
                    string vname = vv.Attribute("Name").Value;

                    if(sname==vname)
                    {
                        var afile = GetAssemblyPath(ass);
                        if (System.IO.File.Exists(afile) && !string.IsNullOrEmpty(cls))
                        {
                            var asb = Assembly.LoadFrom(afile).CreateInstance(cls) as IMessageServiceProxy;

                            return asb;

                        }
                        break;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public IMessageServiceProxyDevelop LoadForDevelop(string sname)
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location),"Config", "MessageServerDevelop.cfg");
            if (System.IO.File.Exists(sfile))
            {
                XElement xx = XElement.Load(sfile);
                foreach (var vv in xx.Elements())
                {
                    string ass = vv.Attribute("Assembly").Value;
                    string cls = vv.Attribute("Class").Value;
                    var vname = vv.Attribute("Name").Value;
                    if (vname == sname)
                    {
                        var afile = GetAssemblyPath(ass);
                        if (System.IO.File.Exists(afile) && !string.IsNullOrEmpty(cls))
                        {
                            try
                            {
                                var asb = Assembly.LoadFrom(afile);
                                var css = asb.CreateInstance(cls) as IMessageServiceProxyDevelop;
                                return css;
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                            }

                        }
                        break;
                    }
                }
            }
            return null;
        }

        private void Init()
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "Config", "MessageServerDevelop.cfg");
            if (System.IO.File.Exists(sfile))
            {
                XElement xx = XElement.Load(sfile);
                foreach (var vv in xx.Elements())
                {
                    string ass = vv.Attribute("Assembly").Value;
                    string cls = vv.Attribute("Class").Value;
                    var vname = vv.Attribute("Name").Value;
                    mProxyService.Add(vname);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> ListDevelopApis()
        {
            return mProxyService.ToList();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

    }
}
