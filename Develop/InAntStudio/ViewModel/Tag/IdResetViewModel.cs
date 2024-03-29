﻿//==============================================================
//  Copyright (C) 2021  Inc. All rights reserved.
//
//==============================================================
//  Create by chongdaoyang at 2021/3/3 11:03:15.
//  Version 1.0
//  CHONGDAOYANGPC
//==============================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InAntStudio.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class IdResetViewModel : WindowViewModelBase
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        public IdResetViewModel()
        {
            DefaultWidth = 540;
            DefaultHeight = 140;
            IsEnableMax = false;
            Title = Res.Get("FindAvaiableId");
        }
        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public int StartId { get; set; }


        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
