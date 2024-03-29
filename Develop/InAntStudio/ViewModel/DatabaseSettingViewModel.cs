﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/9/15 15:36:01.
//  Version 1.0
//  种道洋
//==============================================================


using Google.Protobuf.Reflection;
using InAntStudio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseSettingViewModel: TreeItemViewModel
    {

        #region ... Variables  ...
        
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        public DatabaseSettingViewModel()
        {
            Name = Res.Get("Setting");
        }
        #endregion ...Constructor...

        #region ... Properties ...

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override ViewModelBase GetModel(ViewModelBase model)
        {
            if (model is DatabaseSettingConfigViewModel)
            {
                (model as DatabaseSettingConfigViewModel).Database = this.Database;
                return model;
            }
            else
            {
                return new DatabaseSettingConfigViewModel() { Database = this.Database };
            }
        }



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }




}
