﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/3/29 11:10:37.
//  Version 1.0
//  种道洋
//==============================================================


using DBDevelopClientApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace InAntStudio.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ListDatabaseViewModel : WindowViewModelBase
    {

        #region ... Variables  ...
        private DatabaseItem mSelectDatabase;
        private ICommand mNewCommand;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        public ListDatabaseViewModel()
        {
            Init();
            DefaultHeight = 300;
            DefaultWidth = 600;
            Title = Res.Get("databaseSelect");
        }
        #endregion ...Constructor...

        #region ... Properties ...

        ///// <summary>
        ///// 
        ///// </summary>
        //public ICommand NewCommand
        //{
        //    get
        //    {
        //        if (mNewCommand == null)
        //        {
        //            mNewCommand = new RelayCommand(() =>
        //            {
        //                NewDatabase();
        //            });
        //        }
        //        return mNewCommand;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        public DatabaseItem SelectDatabase
        {
            get
            {
                return mSelectDatabase;
            }
            set
            {
                if (mSelectDatabase != value)
                {
                    mSelectDatabase = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<DatabaseItem> DatabaseItems { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        private void Init()
        {
            List<DatabaseItem> re = new List<DatabaseItem>();
            foreach (var vv in DevelopServiceHelper.Helper.ListDatabase())
            {
                re.Add(new DatabaseItem() { Name = vv.Key, Desc = vv.Value });
            }
            this.DatabaseItems = re;
            if (re.Count > 0)
                SelectDatabase = re[0];
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //private void NewDatabase()
        //{
        //    NewDatabaseViewModel ndm = new NewDatabaseViewModel();
        //    if (this.DatabaseItems != null && this.DatabaseItems.Count > 0)
        //        ndm.ExistDatabase = this.DatabaseItems.Select(e => e.Name).ToList();

        //    if (ndm.ShowDialog().Value)
        //    {
        //        this.mSelectDatabase = new DatabaseItem() { Name = ndm.Name, Desc = ndm.Desc };
        //        this.IsOK = true;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool CanOKCommandProcess()
        {
            return mSelectDatabase != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool AutoOpenDatabse()
        {
            SelectDatabase = new DatabaseItem() { Name = AutoLoginHelper.Helper.Database };

            if (!DevelopServiceHelper.Helper.CheckOpenDatabase(AutoLoginHelper.Helper.Database))
            {
                MessageBox.Show(string.Format(Res.Get("opendatabasefailed"), SelectDatabase.Name));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool OKCommandProcess()
        {
            if (!DevelopServiceHelper.Helper.CheckOpenDatabase(SelectDatabase.Name))
            {
                MessageBox.Show(string.Format(Res.Get("opendatabasefailed"), SelectDatabase.Name));
                return false;
            }
            return base.OKCommandProcess();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    public class DatabaseItem
    {
        public string Name { get; set; }

        public string Desc { get; set; }
    }

}
