﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/8/17 10:50:12.
//  Version 1.0
//  种道洋
//==============================================================
using Cdy.Ant.Tag;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProxyServiceFactory
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IMessageServiceProxy GetRuntimeInstance(string type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IMessageServiceProxyDevelop GetDevelopInstance(string type);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IMessageServiceProxyDevelop GetDevelopInstance();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> ListDevelopApis();


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
