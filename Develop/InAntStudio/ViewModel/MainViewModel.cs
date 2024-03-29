﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/3/28 10:54:26.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using InAntStudio.ViewModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

using Microsoft.Win32;
using System.IO;
using Cdy.Ant;
using System.Diagnostics;
using System.Timers;
using DBDevelopClientApi;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : ViewModelBase, IProcessNotify, IRefreshContent
    {

        #region ... Variables  ...

        private ICommand mLoginCommand;

        private string mDatabase = string.Empty;

        private ICommand mSaveCommand;

        private ICommand mStartCommand;

        private ICommand mStopCommand;

        private ICommand mReRunCommand;

        private ICommand mLogoutCommand;

        private ICommand mAddGroupCommand;
        
        private ICommand mRemoveGroupCommand;

        private ICommand mExportCommand;

        private ICommand mImportCommand;

        private ICommand mDatabaseSelectCommand;

        private ICommand mReNameCommand;
        private ICommand mRemoveCommand;

        //private ICommand mNewDatabaseCommand;

        private ICommand mCancelCommand;

        private TreeItemViewModel mCurrentSelectTreeItem;

        private System.Collections.ObjectModel.ObservableCollection<TreeItemViewModel> mTagGroup = new System.Collections.ObjectModel.ObservableCollection<TreeItemViewModel>();


        private RootTagGroupViewModel mRootTagGroupModel = new RootTagGroupViewModel();

        //private SecurityTreeItemViewModel securityModel = new SecurityTreeItemViewModel();

        private ViewModelBase mContentViewModel;

        private bool mIsCanOperate = true;

        private double mProcessNotify;

        private Visibility mNotifyVisiblity = Visibility.Hidden;

        private bool mIsLogin;

        private bool mIsDatabaseRunning;

        private System.Timers.Timer mCheckRunningTimer;

        private MarInfoViewModel infoModel;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        public MainViewModel()
        {
            //ValueConvertManager.manager.Init();

            ApiFactory.Factory.LoadForDevelop();

            ServiceLocator.Locator.Registor<IProcessNotify>(this);
            ServiceLocator.Locator.Registor<IRefreshContent>(this);

            CurrentUserManager.Manager.RefreshNameEvent += Manager_RefreshNameEvent;
            mCheckRunningTimer = new System.Timers.Timer(3000);
            mCheckRunningTimer.Elapsed += MCheckRunningTimer_Elapsed;
            infoModel = new MarInfoViewModel();

            mContentViewModel = infoModel;

            mRootTagGroupModel.Owner = this;
        }

        
        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public bool IsManualLogin
        {
            get
            {
                return string.IsNullOrEmpty(AutoLoginHelper.Helper.Server);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDatabaseRunning
        {
            get
            {
                return mIsDatabaseRunning;
            }
            set
            {
                if (mIsDatabaseRunning != value)
                {
                    mIsDatabaseRunning = value;
                    OnPropertyChanged("IsDatabaseRunning");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ReRunCommand
        {
            get
            {
                if(mReRunCommand==null)
                {
                    mReRunCommand = new RelayCommand(() => {
                        CheckSaveDatabase();
                        DevelopServiceHelper.Helper.ReRunDatabase(mDatabase);
                    },()=> { return !string.IsNullOrEmpty(mDatabase)&& mIsDatabaseRunning; });
                }
                return mReRunCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand StartCommand
        {
            get
            {
                if (mStartCommand == null)
                {
                    mStartCommand = new RelayCommand(() => {
                        CheckSaveDatabase();
                      IsDatabaseRunning =  DevelopServiceHelper.Helper.StartDatabase(mDatabase);
                    }, () => { return !mIsDatabaseRunning&& !string.IsNullOrEmpty(mDatabase); });
                }
                return mStartCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand StopCommand
        {
            get
            {
                if(mStopCommand==null)
                {
                    mStopCommand = new RelayCommand(() => {
                        IsDatabaseRunning = !DevelopServiceHelper.Helper.StopDatabase(mDatabase);
                    },()=> { return mIsDatabaseRunning&& !string.IsNullOrEmpty(mDatabase); });
                }
                return mStopCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if(mCancelCommand==null)
                {
                    mCancelCommand = new RelayCommand(() => { 
                        if(MessageBox.Show(Res.Get("canceltosavemsg"),"",MessageBoxButton.YesNo)== MessageBoxResult.Yes)
                        {
                            DevelopServiceHelper.Helper.CancelToSaveDatabase(mDatabase);

                            if (ContentViewModel is IModeSwitch)
                            {
                                (ContentViewModel as IModeSwitch).Active();
                            }
                        }
                    },()=> { return !string.IsNullOrEmpty(mDatabase); });
                }
                return mCancelCommand;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public string MainwindowTitle
        {
            get
            {
                return string.IsNullOrEmpty(Database) ? Res.Get("MainwindowTitle"): Res.Get("MainwindowTitle")+"--"+this.Database;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string Database
        {
            get
            {
                return mDatabase;
            }
            set
            {
                if (mDatabase != value)
                {
                    mDatabase = value;
                    ServiceHelper.Helper.Database = mDatabase;
                    OnPropertyChanged("Database");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public string UserName
        {
            get
            {
                return CurrentUserManager.Manager.UserName;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public Visibility NotifyVisiblity
        {
            get
            {
                return mNotifyVisiblity;
            }
            set
            {
                if (mNotifyVisiblity != value)
                {
                    mNotifyVisiblity = value;
                    OnPropertyChanged("NotifyVisiblity");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public double ProcessNotify
        {
            get
            {
                return mProcessNotify;
            }
            set
            {
                if (mProcessNotify != value)
                {
                    mProcessNotify = value;
                    OnPropertyChanged("ProcessNotify");
                    OnPropertyChanged("ProcessNotifyPercent");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double ProcessNotifyPercent
        {
            get
            {
                return mProcessNotify / 100;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public bool IsCanOperate
        {
            get
            {
                return mIsCanOperate;
            }
            set
            {
                if (mIsCanOperate != value)
                {
                    mIsCanOperate = value;
                    OnPropertyChanged("IsCanOperate");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ImportCommand
        {
            get
            {
                if(mImportCommand==null)
                {
                    mImportCommand = new RelayCommand(() => {
                        ImportFromFile();
                    }, () => { return IsLogin && !string.IsNullOrEmpty(Database); });
                }
                return mImportCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ExportCommand
        {
            get
            {
                if (mExportCommand == null)
                {
                    mExportCommand = new RelayCommand(() => {
                        ExportToFile();
                    }, () => { return IsLogin && !string.IsNullOrEmpty(Database); });
                }
                return mExportCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand RemoveCommand
        {
            get {
                if(mRemoveCommand==null)
                {
                    mRemoveCommand = new RelayCommand(() => {
                        CurrentSelectGroup.RemoveCommand.Execute(null);
                    }, () => { return CurrentSelectGroup != null && CurrentSelectGroup.RemoveCommand.CanExecute(null); });
                }
                return mRemoveCommand; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ReNameCommand
        {
            get
            {
                if(mReNameCommand==null)
                {
                    mReNameCommand = new RelayCommand(() => {
                        CurrentSelectGroup.ReNameCommand.Execute(null);
                    }, () => { return CurrentSelectGroup != null && CurrentSelectGroup.ReNameCommand.CanExecute(null); });
                }
                return mReNameCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ViewModelBase ContentViewModel
        {
            get
            {
                return mContentViewModel;
            }
            set
            {
                if(mContentViewModel!=value)
                {
                    mContentViewModel = value;
                    OnPropertyChanged("ContentViewModel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<TreeItemViewModel> TagGroup
        {
            get
            {
                return mTagGroup;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public TreeItemViewModel CurrentSelectGroup
        {
            get
            {
                return mCurrentSelectTreeItem;
            }
            set
            {
                if (mCurrentSelectTreeItem != value)
                {
                    mCurrentSelectTreeItem = value;
                    SelectContentModel();
                    OnPropertyChanged("CurrentSelectGroup");
                }
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        public ICommand LogoutCommand
        {
            get
            {
                if(mLogoutCommand==null)
                {
                    mLogoutCommand = new RelayCommand(() => {
                        CheckSaveDatabase();
                        Logout();
                    }, () => { return IsLogin; });
                }
                return mLogoutCommand;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public ICommand LoginCommand
        {
            get
            {
                if (mLoginCommand == null)
                {
                    mLoginCommand = new RelayCommand(() => {
                        Login();
                    });
                }
                return mLoginCommand;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public ICommand AddGroupCommand
        {
            get
            {
                if (mAddGroupCommand == null)
                {
                    mAddGroupCommand = new RelayCommand(() => {
                        (CurrentSelectGroup).AddCommand.Execute(null);
                        //  NewGroup();
                    },()=> { return mCurrentSelectTreeItem != null && mCurrentSelectTreeItem.CanAddChild(); });
                }
                return mAddGroupCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand RemoveGroupCommand
        {
            get
            {
                if (mRemoveGroupCommand == null)
                {
                    mRemoveGroupCommand = new RelayCommand(() => {
                        (CurrentSelectGroup).RemoveCommand.Execute(null);
                    },()=> { return CurrentSelectGroup != null && CurrentSelectGroup.CanRemove() ; });
                }
                return mRemoveGroupCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if(mSaveCommand==null)
                {
                    mSaveCommand = new RelayCommand(() => {

                        CheckSaveDatabase(false);
                        //if (ContentViewModel is TagGroupDetailViewModel)
                        //{
                        //    (ContentViewModel as TagGroupDetailViewModel).UpdateAll();
                        //}
                        //else
                        //{

                        //}

                        if (DevelopServiceHelper.Helper.Save(mDatabase))
                        {
                            MessageBox.Show(Res.Get("SaveSucessful"));
                        }
                        else
                        {
                            MessageBox.Show(Res.Get("Savefailed"));
                        }
                    }, () => { return IsLogin && !string.IsNullOrEmpty(Database); });
                }
                return mSaveCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand DatabaseSelectCommand
        {
            get
            {
                if(mDatabaseSelectCommand==null)
                {
                    mDatabaseSelectCommand = new RelayCommand(() => {
                        CheckSaveDatabase();
                        SwitchDatabase();
                    },()=> { return IsLogin; });
                }
                return mDatabaseSelectCommand;
            }

        }
       
        //public ICommand NewDatabaseCommand
        //{
        //    get
        //    {
        //        if(mNewDatabaseCommand==null)
        //        {
        //            mNewDatabaseCommand = new RelayCommand(() => {
        //                CheckSaveDatabase();
        //                NewDatabase();
        //            }, () => { return IsLogin; });
        //        }
        //        return mNewDatabaseCommand;
        //    }
        //}



        /// <summary>
        /// 
        /// </summary>
        public bool IsLogin
        {
            get
            {
                return mIsLogin;
            }
            set
            {
                if (mIsLogin != value)
                {
                    mIsLogin = value;
                    OnPropertyChanged("IsLogin");
                    OnPropertyChanged("IsLoginOut");
                }
            }
        }

        public bool IsLoginOut
        {
            get
            {
                return !IsLogin;
            }
        }




        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        private void CheckSaveDatabase(bool propmsg=true)
        {
            if (ContentViewModel is IModeSwitch)
            {
                (ContentViewModel as IModeSwitch).DeActive();
            }
            if (DevelopServiceHelper.Helper.IsDatabaseDirty(mDatabase))
            {
                if (propmsg)
                {
                    if (MessageBox.Show(Res.Get("saveprompt"), "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        DevelopServiceHelper.Helper.Save(mDatabase);
                    }
                }
                else
                {
                    DevelopServiceHelper.Helper.Save(mDatabase);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MCheckRunningTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!string.IsNullOrEmpty(mDatabase))
            {
                try
                {
                    var isrunning = DevelopServiceHelper.Helper.IsDatabaseRunning(mDatabase);
                    Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        IsDatabaseRunning = isrunning;
                    }), null);
                }
                catch
                {

                }
            }
        }

        //private void NewDatabase()
        //{
        //    NewDatabaseViewModel ndm = new NewDatabaseViewModel();

        //    var vdd = DevelopServiceHelper.Helper.ListDatabase();
        //    if(vdd.Count>0)
        //    {
        //        ndm.ExistDatabase = vdd.Keys.ToList();
        //    }

        //    if (ndm.ShowDialog().Value)
        //    {
        //        Database = ndm.Name;
        //        OnPropertyChanged("MainwindowTitle");
        //        OnPropertyChanged("UserName");
        //        IsLogin = true;

        //        foreach (var vv in TagGroup) vv.Dispose();
        //        this.TagGroup.Clear();

        //        var dbitem = new DatabaseViewModel() { Name = mDatabase, IsSelected = true, IsExpanded = true };
        //        this.TagGroup.Add(dbitem);

        //        //var sec = new ServerSecurityTreeViewModel();
        //        //sec.Children.Add(new ServerUserEditorTreeViewModel());

        //        //if (DevelopServiceHelper.Helper.IsAdmin())
        //        //{
        //        //    sec.Children.Add(new ServerUserManagerTreeViewModel());
        //        //}

        //        //this.TagGroup.Add(sec);
        //        dbitem.Children.Add(mRootTagGroupModel);
        //        mRootTagGroupModel.Database = mDatabase;

        //        ////dbitem.Children.Add(securityModel);
        //        //securityModel.Database = mDatabase;
        //        //securityModel.Init();

        //        dbitem.Children.Add(new DatabaseSettingViewModel() { Database = this.Database });


        //        Task.Run(() =>
        //        {
        //            //TagViewModel.Drivers = DevelopServiceHelper.Helper.GetRegistorDrivers(mDatabase);
        //            QueryGroups();
        //        });
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        private void ImportFromFile()
        {
            IsCanOperate = false;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "csv|*.csv";
            List<TagViewModel> ltmp = new List<TagViewModel>();

            if (ofd.ShowDialog().Value)
            {
                var stream = new StreamReader(File.Open(ofd.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite));
                while (!stream.EndOfStream)
                {
                    string sval = stream.ReadLine();
                    if (!string.IsNullOrEmpty(sval))
                    {
                        TagViewModel tm = TagViewModel.LoadFromCSVString(sval);
                        ltmp.Add(tm);
                    }
                }
                stream.Close();
            }

            int mode = 0;
            var mm = new ImportModeSelectViewModel();
            if (mm.ShowDialog().Value)
            {
                mode = mm.Mode;
            }
            else
            {
                return;
            }

            StringBuilder sb = new StringBuilder();

            Task.Run(() => {
                BeginShowNotify();
                int id;
                int icount = 0;
                int tcount = ltmp.Count;

                //删除所有，重新添加
                if (mode == 1)
                {
                    DevelopServiceHelper.Helper.ClearTagAll(this.mDatabase);
                }

                bool haserro = false;
                foreach (var vv in ltmp)
                {
                    //更新数据
                    if (!DevelopServiceHelper.Helper.Import(this.mDatabase, vv.RealTagMode, mode, out id))
                    {
                        sb.AppendLine(string.Format(Res.Get("AddTagFail"), vv.RealTagMode.Name));
                        haserro = true;
                    }
                    else
                    {
                        vv.IsNew = false;
                        vv.IsChanged = false;
                    }

                    icount++;
                    ServiceLocator.Locator.Resolve<IProcessNotify>().ShowNotifyValue(((icount * 1.0 / tcount) * 100));
                }

                if (haserro)
                {
                    string errofile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ofd.FileName), "erro.txt");
                    System.IO.File.WriteAllText(errofile, sb.ToString());
                    if (MessageBox.Show(Res.Get("ImportErroMsg"), "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        try
                        {
                            Process.Start(Path.GetDirectoryName(errofile));
                        }
                        catch
                        {

                        }
                    }
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    IsCanOperate = true;
                    QueryGroups();
                    EndShowNotify();
                }));
            });

            

           
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExportToFile()
        {
            IsCanOperate = false;
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "csv|*.csv";
            if (ofd.ShowDialog().Value)
            {

                var stream = new StreamWriter(File.Open(ofd.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite));

                Task.Run(() => {
                    BeginShowNotify();
                    DevelopServiceHelper.Helper.QueryAllTag(mDatabase,new Action<int, int, Dictionary<int, Tagbase>>((idx,total,res)=> {
                        foreach (var vv in res.Select(e => new TagViewModel(e.Value)))
                        {
                            stream.WriteLine(vv.SaveToCSVString());
                           
                            ServiceLocator.Locator.Resolve<IProcessNotify>().ShowNotifyValue(((idx * 1.0 / total) * 100));
                        }

                    }));
                    stream.Close();
                    EndShowNotify();
                    IsCanOperate = true;
                    MessageBox.Show(Res.Get("TagExportComplete"));
                });

                
            }

          
        }

        /// <summary>
        /// 
        /// </summary>
        private void SwitchDatabase()
        {
            ListDatabaseViewModel ldm = new ListDatabaseViewModel();
            if (ldm.ShowDialog().Value)
            {
                if(Database!=ldm.SelectDatabase.Name)
                {
                    if (ContentViewModel is IModeSwitch)
                    {
                        (ContentViewModel as IModeSwitch).DeActive();
                    }

                    Database = ldm.SelectDatabase.Name;
                    OnPropertyChanged("MainwindowTitle");

                    foreach (var vv in this.TagGroup) vv.Dispose();
                    this.TagGroup.Clear();

                    var dbitem = new DatabaseViewModel() { Name = mDatabase, IsSelected = true, IsExpanded = true };
                    this.TagGroup.Add(dbitem);

                    //var sec = new ServerSecurityTreeViewModel();
                    //sec.Children.Add(new ServerUserEditorTreeViewModel() { Name = UserName });

                    //if (DevelopServiceHelper.Helper.IsAdmin())
                    //{
                    //    sec.Children.Add(new ServerUserManagerTreeViewModel());
                    //}

                    //this.TagGroup.Add(sec);
                    dbitem.Children.Add(mRootTagGroupModel);
                    mRootTagGroupModel.Database = mDatabase;
                    //dbitem.Children.Add(securityModel);
                    //securityModel.Database = mDatabase;
                    //securityModel.Init();

                    dbitem.Children.Add(new DatabaseSettingViewModel() { Database = this.Database });

                    Task.Run(() =>
                    {
                        //TagViewModel.Drivers = DevelopServiceHelper.Helper.GetRegistorDrivers(mDatabase);
                        QueryGroups();
                    });

                    IsDatabaseRunning = DevelopServiceHelper.Helper.IsDatabaseRunning(mDatabase);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Manager_RefreshNameEvent(object sender, EventArgs e)
        {
            OnPropertyChanged("UserName");
        }

        /// <summary>
        /// 
        /// </summary>
        private void StartCheckDatabaseRunning()
        {
            mCheckRunningTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopCheckDatabaseRunning()
        {
            mCheckRunningTimer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Logout()
        {
            IsLogin = false;
            foreach (var vv in this.TagGroup)
            {
                vv.Dispose();
            }
            this.TagGroup.Clear();
            CurrentUserManager.Manager.UserName = string.Empty;
            OnPropertyChanged("UserName");
            if (ContentViewModel != null)
            {
                
                ContentViewModel.Dispose();
            }

            ContentViewModel = infoModel;
            Database = string.Empty;
            StopCheckDatabaseRunning();
        }

        /// <summary>
        /// 
        /// </summary>
        public void AutoLogin()
        {
            LoginViewModel login = new LoginViewModel();
            if(login.AutoLogin())
            {
                CurrentUserManager.Manager.UserName = login.UserName;
                ListDatabaseViewModel ldm = new ListDatabaseViewModel();
                if(ldm.AutoOpenDatabse())
                {
                    this.TagGroup.Clear();

                    CurrentUserManager.Manager.UserName = login.UserName;
                    Database = ldm.SelectDatabase.Name;
                    OnPropertyChanged("MainwindowTitle");
                    OnPropertyChanged("UserName");
                    IsLogin = true;

                    var dbitem = new DatabaseViewModel() { Name = mDatabase, IsSelected = true, IsExpanded = true };
                    this.TagGroup.Add(dbitem);

                    dbitem.Children.Add(mRootTagGroupModel);
                    mRootTagGroupModel.Database = mDatabase;

                    dbitem.Children.Add(new DatabaseSettingViewModel() { Database = this.Database });

                    Task.Run(() => {
                        QueryGroups();
                    });

                    StartCheckDatabaseRunning();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Login()
        {
            LoginViewModel login = new LoginViewModel();
            if (login.ShowDialog().Value)
            {
                ListDatabaseViewModel ldm = new ListDatabaseViewModel();
                if (ldm.ShowDialog().Value)
                {

                   

                    this.TagGroup.Clear();
                    
                    CurrentUserManager.Manager.UserName = login.UserName;
                    Database = ldm.SelectDatabase.Name;
                    OnPropertyChanged("MainwindowTitle");
                    OnPropertyChanged("UserName");
                    IsLogin = true;

                    var dbitem = new DatabaseViewModel() { Name = mDatabase,IsSelected=true,IsExpanded=true };
                    this.TagGroup.Add(dbitem);
                    dbitem.Children.Add(mRootTagGroupModel);
                    mRootTagGroupModel.Database = mDatabase;
                    dbitem.Children.Add(new DatabaseSettingViewModel() { Database = this.Database });

                    Task.Run(() => {
                        QueryGroups();
                    });

                    StartCheckDatabaseRunning();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void QueryGroups()
        {
            Application.Current?.Dispatcher.Invoke(() => {
                this.mRootTagGroupModel.Children.Clear();
            });
            
            var vv = DevelopServiceHelper.Helper.QueryTagGroups(this.mDatabase);
            if(vv!=null)
            {
                foreach(var vvv in vv.Where(e=>string.IsNullOrEmpty(e.Item3)))
                {
                    Application.Current?.Dispatcher.Invoke(() => {
                        TagGroupViewModel groupViewModel = new TagGroupViewModel() { mName = vvv.Item1,Database=mDatabase,Parent = mRootTagGroupModel,mDescription=vvv.Item2 };
                        mRootTagGroupModel.Children.Add(groupViewModel);
                        groupViewModel.InitData(vv);
                    });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SelectContentModel()
        {
            if (ContentViewModel is IModeSwitch)
            {
                (ContentViewModel as IModeSwitch).DeActive();
            }

            if (mCurrentSelectTreeItem != null)
                ContentViewModel = mCurrentSelectTreeItem.GetModel(ContentViewModel);
            else
                ContentViewModel = null;

            if (ContentViewModel == null) ContentViewModel = infoModel;

            if (ContentViewModel is IModeSwitch)
            {
                (ContentViewModel as IModeSwitch).Active();
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        public void BeginShowNotify()
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => {

                NotifyVisiblity = Visibility.Visible;
            }));
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void ShowNotifyValue(double val)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => {
                if (val > 100)
                {
                    ProcessNotify = 100;
                }
                else
                {
                    ProcessNotify = val;
                }
            }));
            
        }

        /// <summary>
        /// 
        /// </summary>
        public void EndShowNotify()
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => {
                NotifyVisiblity = Visibility.Hidden;
                ProcessNotify = 0;
            }));
           
        }

        public void RefreshContent()
        {
            if(ContentViewModel!=null && ContentViewModel is IModeSwitch)
            {
                (ContentViewModel as IModeSwitch).Active();
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //public interface ITagGroupAdd
    //{
    //    bool AddGroup(string parent);
    //}

    /// <summary>
    /// 
    /// </summary>
    public interface IProcessNotify
    {
        /// <summary>
        /// 
        /// </summary>
        void BeginShowNotify();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        void ShowNotifyValue(double val);
        /// <summary>
        /// 
        /// </summary>
        void EndShowNotify();
    }


    public interface IRefreshContent
    {
        void RefreshContent();
    }


}
