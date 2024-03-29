﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/2/16 10:57:27.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace AntRuntime
{
    /// <summary>
    /// 
    /// </summary>
    public class UserPermission
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Desc { get; set; } = "";

        /// <summary>
        /// 允许修改
        /// </summary>
        public bool EnableWrite { get; set; }

        /// <summary>
        /// 超级权限
        /// </summary>
        public bool SuperPermission { get; set; }

        /// <summary>
        /// 允许访问的变量组
        /// </summary>
        public List<string> Group { get; set; } = new List<string>();
    }
}
