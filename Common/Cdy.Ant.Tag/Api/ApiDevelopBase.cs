﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/8/17 9:45:00.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ApiDevelopBase : IDataTagApiDevelop, IApiDevelopForFactory
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...


        /// <summary>
        /// 
        /// </summary>
        public abstract ApiData Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract string TypeName { get; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 通过界面配置
        /// </summary>
        public virtual object Config()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ApiData CreatNewData()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xe"></param>
        public void Load(XElement xe)
        {
            Data = CreatNewData();
            Data.LoadFromXML(xe);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement Save()
        {
            var vv = Data.SaveToXML();
            vv.SetAttributeValue("TypeName", TypeName);
            return vv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract IDataTagApiDevelop NewApi();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string ConfigTags()
        {
            return string.Empty;
        }


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...




    }
}
