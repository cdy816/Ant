//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/8/10 12:41:13.
//  Version 1.0
//  种道洋
//==============================================================

using Cdy.Ant;
using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace AntRuntime
{
    /// <summary>
    /// 
    /// </summary>
    public class ProxyManager
    {

        #region ... Variables  ...

        /// <summary>
        /// 
        /// </summary>
        public static ProxyManager Manager = new ProxyManager();

        /// <summary>
        /// 
        /// </summary>
        private IMessageServiceProxy mApis;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IMessageServiceProxy Apis { get { return mApis; } }

        #endregion ...Properties...

        #region ... Methods    ...


        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "Data",Name, "ProxyServer.cfg");
            Load(sfile);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sfile"></param>
        public void Load(string sfile)
        {
            if (System.IO.File.Exists(sfile))
            {
                XElement xx = XElement.Load(sfile);
                var doc = xx.Element("Server");
                string sname = doc.Attribute("Name").Value;
                XElement config = doc.Element("Config");
                mApis = ProxyServiceFactory.Factory.GetRuntimeInstance(sname);
                mApis.Load(config);
            }
        }



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
