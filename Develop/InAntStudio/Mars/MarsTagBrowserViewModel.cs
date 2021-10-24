//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/9/8 9:29:38.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class MarsTagBrowserViewModel:WindowViewModelBase
    {

        #region ... Variables  ...
        
        private string mCurrentDatabase;

        DBDevelopClientWebApi.DevelopServiceHelper mHelper;

        private List<string> mDatabase;

        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<MarsTagGroupViewModel> mTagGroups = new ObservableCollection<MarsTagGroupViewModel>();

        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<MarsTagViewModel> mTags = new ObservableCollection<MarsTagViewModel>();

        private Dictionary<string, string> mFilters = new Dictionary<string, string>();

        private MarsTagGroupViewModel mCurrentGroup;

        private MarsTagViewModel mCurrentSelectTag;

        private ICommand mConnectCommand;

        private static string mServerAddress="127.0.0.1:9000";

        private static string mServerPassword="Admin";

        private static string mServerUserName="Admin";

        private bool mIsConnected;

        private bool mEnableFilter = false;

        private bool mRequery = true;

        private int mCurrentPageIndex = 0;

        private bool mIsBusy = false;


        private string mFilterKeyName = string.Empty;

        private bool mTagTypeFilterEnable;

        private int mFilterType = -1;

        private bool mReadWriteModeFilterEnable;

        private int mFilterReadWriteMode = -1;

        public static string[] mTagTypeList;
        public static string[] mReadWriteModeList;

        private int mLastPageCount = 0;

        private static int mServerModel = 1;

        private Cdy.Tag.RealDatabase mLocalDatabase;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        static MarsTagBrowserViewModel()
        {
            mTagTypeList = Enum.GetNames(typeof(Cdy.Tag.TagType));
            mReadWriteModeList = Enum.GetNames(typeof(Cdy.Tag.ReadWriteMode)).Select(e => Res.Get(e)).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        public MarsTagBrowserViewModel()
        {
            Title = Res.Get("MarsTagBrowser");
            DefaultWidth = 1024;
            DefaultHeight = 600;
            LoadConfig();
        }

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
            /// 
            /// </summary>
        public int ServerModel
        {
            get
            {
                return mServerModel;
            }
            set
            {
                if (mServerModel != value)
                {
                    mServerModel = value;

                    OnPropertyChanged("ServerModel");
                    OnPropertyChanged("IsLocalServer");
                    OnPropertyChanged("IsRemoteServer");

                    if (IsLocalServer)
                    {
                        LoadFromLocal();
                    }
                    else
                    {
                        ConnectCommand.Execute(null);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLocalServer
        {
            get
            {
                return ServerModel == 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRemoteServer
        {
            get
            {
                return ServerModel == 1;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public string[] TagTypeList
        {
            get
            {
                return mTagTypeList;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] ReadWriteModeList
        {
            get
            {
                return mReadWriteModeList;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<MarsTagGroupViewModel> TagGroups
        {
            get
            {
                return mTagGroups;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<MarsTagViewModel> Tags
        {
            get
            {
                return mTags;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return mIsConnected;
            }
            set
            {
                if (mIsConnected != value)
                {
                    mIsConnected = value;
                    OnPropertyChanged("IsConnected");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string ServerPassword
        {
            get
            {
                return mServerPassword;
            }
            set
            {
                if (mServerPassword != value)
                {
                    mServerPassword = value;
                    OnPropertyChanged("ServerPassword");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string ServerAddress
        {
            get
            {
                return mServerAddress;
            }
            set
            {
                if (mServerAddress != value)
                {
                    mServerAddress = value;
                    if(!value.Contains(":"))
                    {
                        mServerAddress = value + ":9000";
                    }
                    OnPropertyChanged("ServerAddress");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public string ServerUserName
        {
            get
            {
                return mServerUserName;
            }
            set
            {
                if (mServerUserName != value)
                {
                    mServerUserName = value;
                    OnPropertyChanged("ServerUserName");
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public ICommand ConnectCommand
        {
            get
            {
                if(mConnectCommand==null)
                {
                    mConnectCommand = new RelayCommand(() => {
                        if (!IsConnected)
                        {
                            Load();
                        }
                        else
                        {
                            UnLoad();
                        }
                    });
                }
                return mConnectCommand;
            }
        }

        public string ConnectString
        {
            get
            {
                if(IsConnected)
                {
                    return Res.Get("DisConnect");
                }
                else
                {
                    return Res.Get("Connect");
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public MarsTagViewModel CurrentSelectTag
        {
            get
            {
                return mCurrentSelectTag;
            }
            set
            {
                if (mCurrentSelectTag != value)
                {
                    mCurrentSelectTag = value;
                    OnPropertyChanged("CurrentSelectTag");
                }
            }
        }

       


        ///// <summary>
        ///// 
        ///// </summary>
        //public MarsApiDevelop ApiDevelop { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CurrentDatabase
        {
            get
            {
                return mCurrentDatabase;
            }
            set
            {
                if (mCurrentDatabase != value)
                {
                    mCurrentDatabase = value;
                    mCurrentGroup = null;

                    UpdateTagGroup();
                    NewQueryTags();
                    OnPropertyChanged("CurrentDatabase");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Databases 
        { 
            get { return mDatabase; }
            set 
            { 
                mDatabase = value;
                OnPropertyChanged("Databases");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MarsTagGroupViewModel CurrentGroup
        {
            get
            {
                return mCurrentGroup;
            }
            set
            {
                if (mCurrentGroup != value)
                {
                    mIsBusy = false;
                    mCurrentGroup = value;
                    value.IsSelected = true;
                    NewQueryTags();
                    OnPropertyChanged("CurrentGroup");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataGrid Grid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SelectTagName
        {
            get
            {
                return CurrentSelectTag != null ? CurrentSelectTag.FullName.Replace(CurrentDatabase+".","") : string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLoading { get; set; }

        /// <summary>
            /// 
            /// </summary>
        public bool EnableFilter
        {
            get
            {
                return mEnableFilter;
            }
            set
            {
                if (mEnableFilter != value)
                {
                    mEnableFilter = value;
                    OnPropertyChanged("EnableFilter");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int FilterReadWriteMode
        {
            get
            {
                return mFilterReadWriteMode;
            }
            set
            {
                if (mFilterReadWriteMode != value)
                {
                    mFilterReadWriteMode = value;
                    NewQueryTags();
                }
                OnPropertyChanged("FilterReadWriteMode");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool ReadWriteModeFilterEnable
        {
            get
            {
                return mReadWriteModeFilterEnable;
            }
            set
            {
                if (mReadWriteModeFilterEnable != value)
                {
                    mReadWriteModeFilterEnable = value;
                    if (!value)
                    {
                        mFilterReadWriteMode = -1;
                        NewQueryTags();
                    }
                    OnPropertyChanged("ReadWriteModeFilterEnable");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int FilterType
        {
            get
            {
                return mFilterType;
            }
            set
            {
                if (mFilterType != value)
                {
                    mFilterType = value;
                    NewQueryTags();
                }
                OnPropertyChanged("FilterType");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string FilterKeyName
        {
            get
            {
                return mFilterKeyName;
            }
            set
            {
                if (mFilterKeyName != value)
                {
                    mFilterKeyName = value;
                    NewQueryTags();
                }
                OnPropertyChanged("FilterKeyName");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool TagTypeFilterEnable
        {
            get
            {
                return mTagTypeFilterEnable;
            }
            set
            {
                if (mTagTypeFilterEnable != value)
                {
                    mTagTypeFilterEnable = value;
                    if (!value)
                    {
                        mFilterType = -1;
                        NewQueryTags();
                    }
                }
                OnPropertyChanged("TagTypeFilterEnable");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PreLoadDatabase { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        private void NewQueryTags()
        {
            EnableFilter = false;
            //Task.Run(() => {
            BuildFilters();
            mRequery = true;
            if (IsLocalServer)
            {
                ContinueQueryLocalTags();
            }
            else
            {
                ContinueQueryTags();
            }
            Application.Current?.Dispatcher.Invoke(new Action(() =>
            {
                EnableFilter = true;
            }));
            //});
        }

        /// <summary>
        /// 
        /// </summary>
        private void BuildFilters()
        {
            mFilters.Clear();
            if (!string.IsNullOrEmpty(this.FilterKeyName))
            {
                mFilters.Add("keyword", FilterKeyName);
            }
            if (this.TagTypeFilterEnable)
            {
                mFilters.Add("type", this.FilterType.ToString());
            }
            if (this.ReadWriteModeFilterEnable)
            {
                mFilters.Add("readwritetype", FilterReadWriteMode.ToString());
            }


            //string stmp = "";
            //if (this.DriverFilterEnable)
            //{
            //    stmp = this.FilterDriver;
            //}
            //if (this.RegistorFilterEnable)
            //{
            //    stmp += "." + this.FilterRegistorName;
            //}
            //if (!string.IsNullOrEmpty(stmp))
            //{
            //    mFilters.Add("linkaddress", stmp);
            //}

        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadConfig()
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location) , "MarsApiConfig.cach");
            if(System.IO.File.Exists(sfile))
            {
                XElement xx = XElement.Load(sfile);
                this.ServerAddress = xx.Attribute("ServerAddress")?.Value;
                this.ServerUserName = xx.Attribute("ServerUserName")?.Value;
                //this.ServerPassword = xx.Attribute("ServerPassword")?.Value;
            }
        }

        /// <summary>
        /// 从本地文件加载数据库
        /// </summary>
        public void LoadFromLocal()
        {
            string spath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "TagBrowserCach");

            if(!System.IO.Directory.Exists(spath))
            {
                System.IO.Directory.CreateDirectory(spath);
            }

            Databases = (new System.IO.DirectoryInfo(spath)).EnumerateDirectories().Select(e => e.Name).ToList();
            if (Databases.Count > 0)
            {
                CurrentDatabase = Databases[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            try
            {
                mHelper = new DBDevelopClientWebApi.DevelopServiceHelper();
                mHelper.Server = ServerAddress;
                if (!mHelper.Server.StartsWith("http://"))
                {
                    mHelper.Server = "http://" + mHelper.Server;
                }
                if (mHelper.Login(ServerUserName, ServerPassword))
                {
                    IsConnected = true;
                    Databases = mHelper.QueryDatabase().Select(e => e.Name).ToList();

                    if (Databases.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(PreLoadDatabase) && Databases.Contains(PreLoadDatabase))
                        {
                            CurrentDatabase = PreLoadDatabase;
                        }
                        else
                        {
                            CurrentDatabase = Databases[0];
                        }
                    }
                }
                else
                {
                    Databases = new List<string>();
                    CurrentGroup = null;
                    IsConnected = false;
                    CommandManager.InvalidateRequerySuggested();
                    MessageBox.Show("Logging server failed!");
                }
                OnPropertyChanged("ConnectString");
            }
            catch(Exception ex)
            {
                IsConnected = false;
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnLoad()
        {
            mHelper.Logout();
            CurrentDatabase = string.Empty;
            Databases = new List<string>();
            IsConnected = false;
            OnPropertyChanged("ConnectString");
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateTagGroup()
        {
            if (IsLocalServer)
            {
                Task.Run(() => { QueryGroupsFromLocal(); });
            }
            else
            {
                Task.Run(() => { QueryGroups(); });
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        private void QueryGroups()
        {
            Application.Current?.Dispatcher.Invoke(() => {
                this.mTagGroups.Clear();
            });
            
            if (!string.IsNullOrEmpty(CurrentDatabase))
            {
                MarsTagGroupViewModel root=null;
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    root = new MarsTagGroupViewModel() { Database = CurrentDatabase, Name = CurrentDatabase,IsExpended=true,IsSelected=true };
                    mTagGroups.Add(root);

                    CurrentGroup = root;
                });
                
                var vv = mHelper.GetTagGroup(CurrentDatabase);
                if (vv != null)
                {
                    foreach (var vvv in vv.Where(e => string.IsNullOrEmpty(e.Parent)))
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            MarsTagGroupViewModel groupViewModel = new MarsTagGroupViewModel() { Name = vvv.Name, Database = CurrentDatabase,Parent=root };
                            root.Children.Add(groupViewModel);
                            groupViewModel.InitData(vv);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void QueryGroupsFromLocal()
        {
            Application.Current?.Dispatcher.Invoke(() => {
                this.mTagGroups.Clear();
            });

            if (!string.IsNullOrEmpty(CurrentDatabase))
            {
                mLocalDatabase = LoadDatabase(CurrentDatabase);

                MarsTagGroupViewModel root = null;
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    root = new MarsTagGroupViewModel() { Database = CurrentDatabase, Name = CurrentDatabase, IsExpended = true, IsSelected = true };
                    mTagGroups.Add(root);

                    CurrentGroup = root;
                });

               
                if(mLocalDatabase!=null)
                {
                    var vv = mLocalDatabase.Groups;

                    foreach (var vvv in vv.Where(e => e.Value.Parent==null))
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            MarsTagGroupViewModel groupViewModel = new MarsTagGroupViewModel() { Name = vvv.Value.Name, Database = CurrentDatabase, Parent = root };
                            root.Children.Add(groupViewModel);
                            groupViewModel.InitData(vv.Values.ToList());
                        });
                    }

                    //CurrentGroup = root;
                }
            }
        }

        private Cdy.Tag.RealDatabase LoadDatabase(string database)
        {
            string spath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "TagBrowserCach",database,database+ ".xdb");
            Cdy.Tag.RealDatabaseSerise rd = new Cdy.Tag.RealDatabaseSerise();
            return rd.Load(spath);
        }


        /// <summary>
        /// 
        /// </summary>
        public void ContinueLoadData()
        {
            if (!IsLoading)
            {
                IsLoading = true;
                System.Threading.Tasks.Task.Run(() => { ContinueQueryTags(); IsLoading = false; });
            }
        }



        ///// <summary>
        ///// 
        ///// </summary>
        //private void UpdateTags()
        //{
        //    mTags.Clear();
        //    var tags = mHelper.GetTagByGroup(CurrentDatabase, CurrentGroup != null ? CurrentGroup.FullName : "", 0);
        //    if (tags != null)
        //    {
        //        foreach (var vv in tags)
        //        {
        //            var vtag = new TagViewModel() { Name = vv.Item1.Name, Desc = vv.Item1.Desc, Type = vv.Item1.Type.ToString(), ReadWriteMode = vv.Item1.ReadWriteType.ToString(), Group = CurrentGroup != null ? CurrentGroup.FullName : "" };
        //            if (vv.Item1 is Cdy.Tag.NumberTagBase)
        //            {
        //                vtag.MaxValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MaxValue;
        //                vtag.MinValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MinValue;
        //            }
        //            if (vv.Item1 is Cdy.Tag.FloatingTagBase)
        //            {
        //                vtag.Precision = (vv.Item1 as Cdy.Tag.FloatingTagBase).Precision;
        //            }
        //            mTags.Add(vtag);
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        private void ContinueQueryTags()
        {
            if (mIsBusy) return;

            mIsBusy = true;
            try
            {
                if (mRequery)
                {
                    mRequery = false;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        mTags.Clear();
                    }));
                    mCurrentPageIndex = 0;

                    string group = CurrentGroup != null ? CurrentGroup.FullName.Replace(CurrentDatabase + ".", "") : "";
                    if(group == CurrentDatabase)
                    {
                        group = "";
                    }

                    var tags = mHelper.GetTagByGroup(CurrentDatabase, group, mCurrentPageIndex,out mLastPageCount, mFilters);
                    if (tags != null)
                    {
                        foreach (var vv in tags)
                        {
                            var vtag = new MarsTagViewModel() { Name = vv.Item1.Name, Desc = vv.Item1.Desc, Type = vv.Item1.Type.ToString(), ReadWriteMode = vv.Item1.ReadWriteType.ToString(), Group = CurrentGroup != null ? CurrentGroup.FullName : "" };
                            if (vv.Item1 is Cdy.Tag.NumberTagBase)
                            {
                                vtag.MaxValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MaxValue;
                                vtag.MinValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MinValue;
                            }
                            if (vv.Item1 is Cdy.Tag.FloatingTagBase)
                            {
                                vtag.Precision = (vv.Item1 as Cdy.Tag.FloatingTagBase).Precision;
                            }

                            Application.Current?.Dispatcher.Invoke(new Action(() =>
                            {
                                mTags.Add(vtag);
                            }));

                        }
                    }
                }
                else
                {
                    if (mCurrentPageIndex >= mLastPageCount) return;

                    mCurrentPageIndex++;
                    string group = CurrentGroup != null ? CurrentGroup.FullName.Replace(CurrentDatabase + ".", "") : "";
                    if (group == CurrentDatabase)
                    {
                        group = "";
                    }

                    var tags = mHelper.GetTagByGroup(CurrentDatabase, group, mCurrentPageIndex, out mLastPageCount, mFilters);
                    if (tags != null && tags.Count > 0)
                    {
                        foreach (var vv in tags)
                        {
                            var vtag = new MarsTagViewModel() { Name = vv.Item1.Name, Desc = vv.Item1.Desc, Type = vv.Item1.Type.ToString(), ReadWriteMode = vv.Item1.ReadWriteType.ToString(), Group = CurrentGroup != null ? CurrentGroup.FullName : "" };
                            if (vv.Item1 is Cdy.Tag.NumberTagBase)
                            {
                                vtag.MaxValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MaxValue;
                                vtag.MinValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MinValue;
                            }
                            if (vv.Item1 is Cdy.Tag.FloatingTagBase)
                            {
                                vtag.Precision = (vv.Item1 as Cdy.Tag.FloatingTagBase).Precision;
                            }

                            Application.Current?.Dispatcher.Invoke(new Action(() =>
                            {
                                mTags.Add(vtag);
                            }));

                        }
                    }
                    else
                    {
                        mCurrentPageIndex--;
                    }
                }
            }
            catch
            {

            }
            mIsBusy = false;
        }


        private void ContinueQueryLocalTags()
        {
            if (mIsBusy) return;

            mIsBusy = true;
            try
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    mTags.Clear();
                }));
                mCurrentPageIndex = 0;

                string group = CurrentGroup != null ? CurrentGroup.FullName.Replace(CurrentDatabase + ".", "") : "";
                if (group == CurrentDatabase)
                {
                    group = "";
                }

                if (mLocalDatabase == null) return;
                //var tags = mHelper.GetTagByGroup(CurrentDatabase, group, mCurrentPageIndex, out mLastPageCount, mFilters);

                var tags = mLocalDatabase.Tags.Values.Where(e=>e.Group ==group);


                if (tags != null)
                {
                    var retags = tags.AsEnumerable();
                    foreach (var vv in mFilters)
                    {
                        if (vv.Key == "keyword")
                        {
                            retags = retags.Where(e => e.Name.Contains(vv.Value) || e.Desc.Contains(vv.Value)).ToList();
                        }
                        else if (vv.Key == "type")
                        {
                            retags = retags.Where(e => (byte)e.Type == int.Parse(vv.Value));
                        }
                        else if (vv.Key == "readwritetype")
                        {
                            retags = retags.Where(e => (byte)e.ReadWriteType == int.Parse(vv.Value));
                        }
                    }
                    foreach (var vv in retags)
                    {
                        var vtag = new MarsTagViewModel() { Name = vv.Name, Desc = vv.Desc, Type = vv.Type.ToString(), ReadWriteMode = vv.ReadWriteType.ToString(), Group = CurrentGroup != null ? CurrentGroup.FullName : "" };
                        if (vv is Cdy.Tag.NumberTagBase)
                        {
                            vtag.MaxValue = (vv as Cdy.Tag.NumberTagBase).MaxValue;
                            vtag.MinValue = (vv as Cdy.Tag.NumberTagBase).MinValue;
                        }
                        if (vv is Cdy.Tag.FloatingTagBase)
                        {
                            vtag.Precision = (vv as Cdy.Tag.FloatingTagBase).Precision;
                        }

                        Application.Current?.Dispatcher.Invoke(new Action(() =>
                        {
                            mTags.Add(vtag);
                        }));

                    }
                }
            }
            catch
            {

            }
            mIsBusy = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MarsTagViewModel> GetSelectTags()
        {
            List<MarsTagViewModel> re = new List<MarsTagViewModel>();
            foreach(MarsTagViewModel vv in Grid.SelectedItems)
            {
                re.Add(vv);
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool CanOKCommandProcess()
        {
            return CurrentSelectTag!=null;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveConfig()
        {
            string sfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "MarsApiConfig.cach");
            XElement xx = new XElement("MarsApiConfig");
            xx.SetAttributeValue("ServerAddress", ServerAddress);
            xx.SetAttributeValue("ServerUserName", ServerUserName);
            xx.Save(sfile);
            //xx.SetAttributeValue("ServerPassword", ServerPassword);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool OKCommandProcess()
        {
            SaveConfig();
            return base.OKCommandProcess();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ClosedProcess()
        {
            try
            {
                if (mHelper != null)
                    mHelper.Logout();
            }
            catch
            {

            }
            base.ClosedProcess();
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class MarsTagGroupViewModel : ViewModelBase
    {

        #region ... Variables  ...
        
        private bool mIsSelected;

        private bool mIsExpended;

        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<MarsTagGroupViewModel> mChildren = new ObservableCollection<MarsTagGroupViewModel>();

        //private bool mIsInited = false;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<MarsTagGroupViewModel> Children
        {
            get
            {
                return mChildren;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string FullName
        {
            get
            {
                return Parent != null ? Parent.FullName + "." + this.Name : this.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MarsTagGroupViewModel Parent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsExpended
        {
            get
            {
                return mIsExpended;
            }
            set
            {
                if (mIsExpended != value)
                {
                    mIsExpended = value;
                    OnPropertyChanged("IsExpended");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groups"></param>
        public void InitData(List<DBDevelopClientWebApi.TagGroup> groups)
        {
            foreach (var vv in groups.Where(e => e.Parent == this.FullName))
            {
                MarsTagGroupViewModel groupViewModel = new MarsTagGroupViewModel() { Name = vv.Name, Database = Database };
                groupViewModel.Parent = this;
                groupViewModel.InitData(groups);
                this.Children.Add(groupViewModel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groups"></param>
        public void InitData(List<Cdy.Tag.TagGroup> groups)
        {
            foreach (var vv in groups.Where(e => e.Parent!=null && e.Parent.FullName == this.FullName))
            {
                MarsTagGroupViewModel groupViewModel = new MarsTagGroupViewModel() { Name = vv.Name, Database = Database };
                groupViewModel.Parent = this;
                groupViewModel.InitData(groups);
                this.Children.Add(groupViewModel);
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class MarsTagViewModel : ViewBase
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
        public string Group { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ReadWriteMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(Group) ? Name : Group + "." + Name;
            }
        }

        public double MaxValue { get; set; }

        public double MinValue { get; set; }

        public int Precision { get; set; }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TagViewModel ConvertTo()
        {
            Cdy.Ant.Tagbase tb = null;
            switch (this.Type)
            {
                case "Bool":
                    tb = new Cdy.Ant.DigitalAlarmTag();
                    break;
                case "Byte":
                case "Short":
                case "UShort":
                case "Int":
                case "Long":
                case "UInt":
                case "ULong":
                case "Float":
                case "Double":
                    tb = new Cdy.Ant.AnalogAlarmTag();
                    break;
                case "String":
                    tb = new Cdy.Ant.StringAlarmTag();
                    break;
            }

            if (tb != null)
            {
                tb.Name = this.Name;
                tb.Desc = this.Desc;
                if (tb is Cdy.Ant.AlarmTag)
                {
                    (tb as Cdy.Ant.AlarmTag).LinkTag = this.FullName;
                }
                return new TagViewModel(tb);
            }
            return null;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

}
