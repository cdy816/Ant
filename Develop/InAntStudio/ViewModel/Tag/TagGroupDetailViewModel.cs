﻿//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/4/4 18:51:48.
//  Version 1.0
//  种道洋
//==============================================================

using Cdy.Ant;
using DBDevelopClientApi;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InAntStudio.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class TagGroupDetailViewModel:ViewModelBase, IModeSwitch
    {

        #region ... Variables  ...
        private TagGroupViewModel mGroupModel;
        private System.Collections.ObjectModel.ObservableCollection<TagViewModel> mSelectGroupTags = new System.Collections.ObjectModel.ObservableCollection<TagViewModel>();

        private ICommand mAddCommand;
        private ICommand mRemoveCommand;
        private ICommand mImportCommand;
        private ICommand mExportCommand;



        private ICommand mImportFromMarsCommand;
        private ICommand mAddFromMarsOnlineCommand;

        private ICommand mCopyCommand;
        private ICommand mCellCopyCommand;
        private ICommand mAlarmCopyCommand;


        private ICommand mPasteCommand;
        private ICommand mCellPasteCommand;
        private ICommand mAlarmPasteCommand;

        private ICommand mReplaceCommand;

        private ICommand mFindAvaiableIdCommand;

        private TagViewModel mCurrentSelectTag;

        private int mTotalPageNumber = 0;
        private int mCurrentPageIndex = 0;

        private bool mIsLoading = false;

        private static List<TagViewModel> mCopyTags = new List<TagViewModel>();

        private int mTagCount = 0;

        private string mFilterKeyName = string.Empty;

        private bool mTagTypeFilterEnable;

        private int mFilterType = -1;


        private Dictionary<string, string> mFilters = new Dictionary<string, string>();

        private bool mEnableFilter = true;

        private Tuple<TagViewModel, int> mPropertyCopy;

        private AlarmConfigDoc mAlarmCopy;

        private DataGridSelectionUnit mSelectMode = DataGridSelectionUnit.FullRow;

        private bool mIsPopup = false;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public bool IsPopup
        {
            get
            {
                return mIsPopup;
            }
            set
            {
                if (mIsPopup != value)
                {
                    mIsPopup = value;
                    OnPropertyChanged("IsPopup");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool RowSelectMode
        {
            get
            {
                return mSelectMode == DataGridSelectionUnit.FullRow;
            }
            set
            {
                mSelectMode = DataGridSelectionUnit.FullRow;
                OnPropertyChanged("RowSelectMode");
                OnPropertyChanged("CellSelectMode");
                OnPropertyChanged("SelectMode");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CellSelectMode
        {
            get
            {
                return mSelectMode == DataGridSelectionUnit.CellOrRowHeader;
            }
            set
            {
                mSelectMode = DataGridSelectionUnit.CellOrRowHeader;
                OnPropertyChanged("CellSelectMode");
                OnPropertyChanged("RowSelectMode");
                OnPropertyChanged("SelectMode");
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public DataGridSelectionUnit SelectMode
        {
            get
            {
                return mSelectMode;
            }
            set
            {
                if (mSelectMode != value)
                {
                    mSelectMode = value;
                    OnPropertyChanged("SelectMode");
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

        private bool mLinkedTagFilterEnable = false;
        private string mLinkedTagFilter = "";

        /// <summary>
        /// 
        /// </summary>
        public bool LinkedTagFilterEnable
        {
            get
            {
                return mLinkedTagFilterEnable;
            }
            set
            {
                if (mLinkedTagFilterEnable != value)
                {
                    mLinkedTagFilterEnable = value;
                    if (!value)
                    {
                        mLinkedTagFilter = "";
                        NewQueryTags();
                    }
                    OnPropertyChanged("LinkedTagFilterEnable");
                }
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        public string LinkedTagFilter
        {
            get
            {
                return mLinkedTagFilter;
            }
            set
            {
                if (mLinkedTagFilter != value)
                {
                    mLinkedTagFilter = value;
                    NewQueryTags();
                    OnPropertyChanged("LinkedTagFilter");
                }
            }
        }




        /// <summary>
        /// 
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return mIsLoading;
            }
            set
            {
                if (mIsLoading != value)
                {
                    mIsLoading = value;
                    OnPropertyChanged("IsLoading");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand CopyCommand
        {
            get
            {
                if(mCopyCommand==null)
                {
                    mCopyCommand = new RelayCommand(() => {
                        CopyTag();
                    },()=> { return RowSelectMode; });
                }
                return mCopyCommand;
            }
         }

        /// <summary>
        /// 
        /// </summary>
        public ICommand CellCopyCommand
        {
            get
            {
                if(mCellCopyCommand==null)
                {
                    mCellCopyCommand = new RelayCommand(() => {
                        CopyTagProperty();
                    },()=> { return CellSelectMode && SelectedCells != null && SelectedCells.Count() >0; });
                }
                return mCellCopyCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand AlarmCopyCommand
        {
            get
            {
                if(mAlarmCopyCommand==null)
                {
                    mAlarmCopyCommand = new RelayCommand(() =>
                    {
                        CopyAlarmConfig();
                    });
                }
                return mAlarmCopyCommand;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public ICommand PasteCommand
        {
            get
            {
                if (mPasteCommand == null)
                {
                    mPasteCommand = new RelayCommand(() => {
                        PasteTag();
                    },()=> { return mCopyTags.Count > 0; });
                }
                return mPasteCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand AlarmPasteCommand
        {
            get
            {
                if(mAlarmPasteCommand==null)
                {
                    mAlarmPasteCommand = new RelayCommand(() => {
                        PasteAlarmConfig();
                    }, () => { return mAlarmCopy != null; });
                }
                return mAlarmPasteCommand;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public ICommand CellPasteCommand
        {
            get
            {
                if(mCellPasteCommand==null)
                {
                    mCellPasteCommand = new RelayCommand(() => {
                        PasteTagProperty();
                    },()=> { return CanPasteTagProperty(); });
                }
                return mCellPasteCommand;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                if (mAddCommand == null)
                {
                    mAddCommand = new RelayCommand(() => {
                        NewTag();
                    });
                }
                return mAddCommand;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public ICommand ImportFromMarsDatabseCommand
        {
            get
            {
                if(mImportFromMarsCommand==null)
                {
                    mImportFromMarsCommand = new RelayCommand(() => {
                        IsPopup = false;
                        ImportFromMarsDatabase();
                    });
                }
                return mImportFromMarsCommand;
            }
        }

       

        /// <summary>
        /// 
        /// </summary>
        public ICommand AddFromMarsOnlineCommand
        {
            get
            {
                if(mAddFromMarsOnlineCommand==null)
                {
                    mAddFromMarsOnlineCommand = new RelayCommand(() => {
                        IsPopup = false;
                        AddFromMarsTag();
                    });
                }
                return mAddFromMarsOnlineCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand RemoveCommand
        {
            get
            {
                if (mRemoveCommand == null)
                {
                    mRemoveCommand = new RelayCommand(() => {
                        RemoveTag();
                    }, () => { return CurrentSelectTag != null || (SelectedCells!=null && SelectedCells.Count>0); });
                }
                return mRemoveCommand;
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
                    });
                }
                return mExportCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ImportCommand
        {
            get
            {
                if (mImportCommand == null)
                {
                    mImportCommand = new RelayCommand(() => {
                        ImportFromFile();
                    });
                }
                return mImportCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand FindAvaiableIdCommand
        {
            get
            {
                if(mFindAvaiableIdCommand==null)
                {
                    mFindAvaiableIdCommand = new RelayCommand(() => {
                        IdResetViewModel model = new IdResetViewModel();
                        if(model.ShowDialog().Value)
                        {
                            DoResetTagId(model.StartId);
                        }
                    });
                }
                return mFindAvaiableIdCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ReplaceCommand
        {
            get
            {
                if(mReplaceCommand==null)
                {
                    mReplaceCommand = new RelayCommand(() =>
                    {

                    }, () => { return CellSelectMode && SelectedCells != null && SelectedCells.Count() > 0; });
                }
                return mReplaceCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TagViewModel CurrentSelectTag
        {
            get
            {
                return mCurrentSelectTag;
            }
            set
            {
                if (mCurrentSelectTag != value)
                {
                    if (mCurrentSelectTag != null && (mCurrentSelectTag.IsChanged || mCurrentSelectTag.IsNew))
                    {
                        UpdateTag(mCurrentSelectTag);
                    }
                    mCurrentSelectTag = value;

                    OnPropertyChanged("CurrentSelectTag");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TagGroupViewModel GroupModel
        {
            get
            {
                return mGroupModel;
            }
            set
            {
                if (mGroupModel != value)
                {
                    mGroupModel = value;
                    mTotalPageNumber = -1;
                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<TagViewModel> SelectGroupTags
        {
            get
            {
                return mSelectGroupTags;
            }
            set
            {
                mSelectGroupTags = value;
                OnPropertyChanged("SelectGroupTags");
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public int TagCount
        {
            get
            {
                return mTagCount;
            }
            set
            {
                if (mTagCount != value)
                {
                    mTagCount = value;
                    OnPropertyChanged("TagCount");
                }
            }
        }

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
        public string[] TagTypeList { get { return TagViewModel.mTagTypeList; } }

        /// <summary>
        /// 
        /// </summary>
        public IList<DataGridCellInfo> SelectedCells { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DataGrid grid { get; set; }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        private void ImportFromMarsDatabase()
        {
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
                        TagViewModel tm = TagViewModel.LoadFromMarsCSVString(sval);
                        if(tm!=null)
                        ltmp.Add(tm);
                    }
                }
                stream.Close();
            }

            foreach(var tm in ltmp)
            {
                if (tm != null)
                {
                    tm.IsNew = true;
                    tm.RealTagMode.Name = GetNewName(tm.RealTagMode.Name);
                    if (this.GroupModel != null && GroupModel is TagGroupViewModel)
                    {
                        tm.RealTagMode.Group = (GroupModel as TagGroupViewModel).FullName;
                    }
                    if (UpdateTag(tm))
                    {
                        this.SelectGroupTags.Add(tm);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddFromMarsTag()
        {
            MarsTagBrowserViewModel mm = new MarsTagBrowserViewModel();
            mm.ServerAddress = ServiceHelper.Helper.Server;
            mm.ServerPassword = ServiceHelper.Helper.Password;
            mm.ServerUserName = ServiceHelper.Helper.UserName;
            mm.PreLoadDatabase = ServiceHelper.Helper.Database;
            List<TagViewModel> ltmp = new List<TagViewModel>();
            if (mm.ShowDialog().Value)
            {
                foreach(var vv in mm.GetSelectTags())
                {
                    var tm = vv.ConvertTo();
                    if(tm!=null)
                    {
                        tm.IsNew = true;
                        tm.RealTagMode.Name = GetNewName(tm.RealTagMode.Name);
                        if (this.GroupModel != null && GroupModel is TagGroupViewModel)
                        {
                            tm.RealTagMode.Group = (GroupModel as TagGroupViewModel).FullName;
                        }
                        if (UpdateTag(tm))
                        {
                            this.SelectGroupTags.Add(tm);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startId"></param>
        private void DoResetTagId(int startId)
        {
            if (CurrentSelectTag != null)
            {
                var res = DevelopServiceHelper.Helper.ResetTagIds(GroupModel.Database, new List<int>() { CurrentSelectTag.Id }, startId);
               if (res!=null && res.Count>0)
                {
                    CurrentSelectTag.Id = res.First().Value;
                }
            }
            else
            {
                var tags = SelectedCells.Select(e => (e.Item as TagViewModel).Id).ToList();
                var res = DevelopServiceHelper.Helper.ResetTagIds(GroupModel.Database, new List<int>() { CurrentSelectTag.Id }, startId);
                if (res != null && res.Count > 0)
                {
                    foreach (var vv in SelectedCells.Select(e => e.Item).Distinct().ToArray())
                    {
                        var vvt = vv as TagViewModel;
                        if(res.ContainsKey(vvt.Id))
                        {
                            vvt.Id = res[vvt.Id];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void NewQueryTags()
        {
            EnableFilter = false;
            UpdateAll();
            Task.Run(() => {
                BuildFilters();
                mTotalPageNumber = -1;
                ContinueQueryTags();
                Application.Current?.Dispatcher.Invoke(new Action(() => {
                    EnableFilter = true;
                }));
            });
        }

        private void BuildFilters()
        {
            mFilters.Clear();
            if(!string.IsNullOrEmpty(this.FilterKeyName))
            {
                mFilters.Add("keyword", FilterKeyName);
            }
            if(this.TagTypeFilterEnable)
            {
                mFilters.Add("type", this.FilterType.ToString());
            }

            string stmp = "";
            if (this.LinkedTagFilterEnable)
            {
                stmp = this.LinkedTagFilter;
            }

            if (!string.IsNullOrEmpty(stmp))
            {
                mFilters.Add("linktag", stmp);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void ExportToFile()
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "csv|*.csv";
            if(ofd.ShowDialog().Value)
            {
                var stream = new StreamWriter(File.Open(ofd.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite));

                Task.Run(() => {
                    ServiceLocator.Locator.Resolve<IProcessNotify>().BeginShowNotify();
                    DevelopServiceHelper.Helper.QueryTagByGroup(GroupModel.Database, GroupModel.FullName,new Action<int, int, Dictionary<int, Tagbase>>((idx, total, res) => {
                        foreach (var vv in res.Select(e => new TagViewModel(e.Value)))
                        {
                            stream.WriteLine(vv.SaveToCSVString());

                            ServiceLocator.Locator.Resolve<IProcessNotify>().ShowNotifyValue(((idx * 1.0 / total) * 100));
                        }

                    }));
                    stream.Close();
                    ServiceLocator.Locator.Resolve<IProcessNotify>().EndShowNotify();

                    MessageBox.Show(Res.Get("TagExportComplete"));
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ImportFromFile()
        {
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
            else
            {
                return;
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

            Task.Run(() =>
            {
                ServiceLocator.Locator.Resolve<IProcessNotify>().BeginShowNotify();

                //删除所有，重新添加
                if (mode == 1)
                {
                    DevelopServiceHelper.Helper.ClearTagByGroup(GroupModel.Database, GroupModel.FullName);
                }

                //var tags = mSelectGroupTags.ToDictionary(e => e.RealTagMode.Name);
                int tcount = ltmp.Count;
                int icount = 0;
                bool haserro = false;
                int id = 0;
                foreach (var vv in ltmp)
                {
                    vv.RealTagMode.Group = this.GroupModel.FullName;

                    //更新数据
                    if (!DevelopServiceHelper.Helper.Import(GroupModel.Database, vv.RealTagMode, mode, out id))
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
                    if(MessageBox.Show(Res.Get("ImportErroMsg"),"",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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

                mCurrentPageIndex = -1;
                mTotalPageNumber = -1;
                ContinueQueryTags();
                Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ServiceLocator.Locator.Resolve<IProcessNotify>().EndShowNotify();
                }));

            });

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

        private bool mIsBusy = false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CanContinueLoadData()
        {
            return mTotalPageNumber < 0 || mCurrentPageIndex < mTotalPageNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ContinueQueryTags()
        {
            if (mIsBusy) return;

            mIsBusy = true;
            int count = 0;
            if (mTotalPageNumber <0)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    SelectGroupTags.Clear();
                }));
                
              //  var vtags = new System.Collections.ObjectModel.ObservableCollection<TagViewModel>();
                mCurrentPageIndex = 0;
                
                var vv = DevelopServiceHelper.Helper.QueryTagByGroup(this.GroupModel.Database, this.GroupModel.FullName,mCurrentPageIndex, out mTotalPageNumber,out count,mFilters);
                if (vv != null)
                {
                    foreach (var vvv in vv)
                    {
                        TagViewModel model = new TagViewModel(vvv.Value);
                        Application.Current?.Dispatcher.Invoke(new Action(() => {
                            SelectGroupTags.Add(model);
                        }));
                       
                    }
                }
                TagCount = count;
            }
            else
            {
                if (mTotalPageNumber > mCurrentPageIndex+1)
                {
                    mCurrentPageIndex++;
                    int totalcount = 0;
                    
                    var vv = DevelopServiceHelper.Helper.QueryTagByGroup(this.GroupModel.Database, this.GroupModel.FullName, mCurrentPageIndex, out totalcount,out count, mFilters);
                    if (vv != null)
                    {
                        foreach (var vvv in vv)
                        {
                            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                TagViewModel model = new TagViewModel(vvv.Value);
                                SelectGroupTags.Add(model);
                            }));
                        }
                    }

                    TagCount = count;
                }
            }
           
            mIsBusy = false;
        }


        /// <summary>
        /// 
        /// </summary>
        private void RemoveTag()
        {
            int icount = 0;
            if (CurrentSelectTag != null)
            {
                int ind = mSelectGroupTags.IndexOf(CurrentSelectTag);

                List<TagViewModel> ll = new List<TagViewModel>();

                foreach(var vv in grid.SelectedItems)
                {
                    ll.Add(vv as TagViewModel);
                }

                foreach(var vvv in ll)
                {
                    if (DevelopServiceHelper.Helper.Remove(GroupModel.Database, vvv.RealTagMode.Id))
                    {
                        SelectGroupTags.Remove(CurrentSelectTag);
                        icount++;
                    }
                }


                if(icount==0)
                {
                    if (DevelopServiceHelper.Helper.Remove(GroupModel.Database, CurrentSelectTag.RealTagMode.Id))
                    {
                        SelectGroupTags.Remove(CurrentSelectTag);
                        icount++;
                    }
                }

                if(ind<mSelectGroupTags.Count)
                {
                    CurrentSelectTag = mSelectGroupTags[ind];
                }
                else
                {
                    if(mSelectGroupTags.Count>0)
                    CurrentSelectTag = mSelectGroupTags.Last();
                    else
                    {
                        CurrentSelectTag = null;
                    }
                }

                //if (DevelopServiceHelper.Helper.Remove(GroupModel.Database, CurrentSelectTag.RealTagMode.Id))
                //{
                //    SelectGroupTags.Remove(CurrentSelectTag);
                //}
            }
            else
            {
                foreach(var vv in SelectedCells.Select(e=>e.Item).Distinct().ToArray())
                {
                    var vvt = vv as TagViewModel;
                    if (DevelopServiceHelper.Helper.Remove(GroupModel.Database, vvt.RealTagMode.Id))
                    {
                        SelectGroupTags.Remove(vvt);
                        icount++;
                    }
                }
            }

            TagCount -= icount;

        }

        /// <summary>
        /// 
        /// </summary>
        private void CopyTag()
        {
            mCopyTags.Clear();

            foreach (var vv in grid.SelectedItems)
            {
                mCopyTags.Add(vv as TagViewModel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CopyAlarmConfig()
        {
            var tm = grid.SelectedItem as TagViewModel;
            if (tm != null) { 
                this.mAlarmCopy = tm.TagDetailModel.CloneAlarmConfig();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CopyTagProperty()
        {
            if(this.SelectedCells.Count()>0)
            {
                var vt = SelectedCells.First();
                var tagproperty = (vt.Item as TagViewModel).Clone();
                
                mPropertyCopy = new Tuple<TagViewModel, int>(tagproperty, vt.Column.DisplayIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PasteTag()
        {
            if(mCopyTags.Count>0)
            {
                TagViewModel tm = null;
                foreach(var vv in mCopyTags)
                {
                    var vtag = vv.Clone();
                    vtag.RealTagMode.Id = -1;
                    vtag.Name = GetNewName(vv.Name);
                    vtag.IsNew = true;
                    if (UpdateTag(vtag))
                    {
                        this.SelectGroupTags.Add(vtag);
                        tm = vtag;
                    }
                }
                if (tm != null)
                    CurrentSelectTag = tm;
            }
            TagCount += mCopyTags.Count;
        }

        private void PasteAlarmConfig()
        {
            var tm = grid.SelectedItem as TagViewModel;
            if (tm != null)
            {
                tm.TagDetailModel.ParsteAlarmConfig(this.mAlarmCopy);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void PasteTagProperty()
        {
            if(mPropertyCopy!=null)
            {
                foreach(var vv in SelectedCells)
                {
                    if(vv.Item == mPropertyCopy.Item1)
                    {
                        continue;
                    }
                    else
                    {
                        TagViewModel tm = vv.Item as TagViewModel;
                        switch (mPropertyCopy.Item2)
                        {
                            case 2:
                                tm.Type = mPropertyCopy.Item1.Type;
                                break;
                            case 3:
                                tm.LinkAddress = mPropertyCopy.Item1.LinkAddress;
                                break;
                            case 4:
                                tm.Desc = mPropertyCopy.Item1.Desc;
                                break;
                            case 5:
                                tm.CustomContent1 = mPropertyCopy.Item1.CustomContent1;
                                break;
                            case 6:
                                tm.CustomContent2 = mPropertyCopy.Item1.CustomContent2;
                                break;
                            case 7:
                                tm.CustomContent3 = mPropertyCopy.Item1.CustomContent3;
                                break;
                            case 8:
                                tm.IsEnable = mPropertyCopy.Item1.IsEnable;
                                break;
                            
                        }

                    }
                }
            }
        }

        private bool CanPasteTagProperty()
        {
            if (mPropertyCopy == null || SelectedCells.Count == 0 || mPropertyCopy.Item2==0) return false;
            foreach(var vv in SelectedCells)
            {
                if(vv.Column.DisplayIndex!=mPropertyCopy.Item2)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void NewTag()
        {
            if (CurrentSelectTag != null)
            {
                var vtag = CurrentSelectTag.Clone();
                vtag.RealTagMode.Id = -1;
                vtag.Name = GetNewName();
                vtag.IsNew = true;
                if (UpdateTag(vtag))
                {
                    this.SelectGroupTags.Add(vtag);
                    CurrentSelectTag = vtag;
                }
            }
            else
            {
                var tag = new Cdy.Ant.AnalogAlarmTag() { Name = GetNewName() };
                if (this.GroupModel != null && GroupModel is TagGroupViewModel)
                {
                    tag.Group = (GroupModel as TagGroupViewModel).FullName;
                }
                var vtag = new TagViewModel(tag) { IsNew = true };
                if (UpdateTag(vtag))
                {
                    this.SelectGroupTags.Add(vtag);
                    CurrentSelectTag = vtag;
                }
            }
            TagCount++;
        }

        /// <summary>
        /// 获取字符串中的数字
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>数字</returns>
        public static int GetNumberInt(string str)
        {
            int result = -1;
            if (str != null && str != string.Empty)
            {
                // 正则表达式剔除非数字字符（不包含小数点.）
                str = Regex.Replace(str, @"[^\d.\d]", "");
                // 如果是数字，则转换为decimal类型
                if (Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(str))
                            result = int.Parse(str);
                    }
                    catch
                    {

                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetNewName(string baseName="tag")
        {
            var vtmps = mSelectGroupTags.Select(e => e.Name).ToList();
            string tagName = baseName;

            if (!vtmps.Contains(tagName))
            {
                return tagName;
            }

            int number = GetNumberInt(baseName);
            if(number>=0)
            {
                if(tagName.EndsWith(number.ToString()))
                {
                    tagName = tagName.Substring(0, tagName.IndexOf(number.ToString()));
                }
            }
            string sname = tagName;
            for (int i = 1; i < int.MaxValue; i++)
            {
                tagName = sname + i;
                if (!vtmps.Contains(tagName))
                {
                    return tagName;
                }
            }
            return tagName;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagmodel"></param>
        private bool UpdateTag(TagViewModel tagmodel)
        {
            if (tagmodel.IsNew)
            {
                int id;
                var re = DevelopServiceHelper.Helper.AddTag(GroupModel.Database, tagmodel.RealTagMode, out id);
                if (re)
                {
                    tagmodel.RealTagMode.Id = id;
        
                    tagmodel.IsChanged = false;
                    tagmodel.IsNew = false;
                }
                return re;
            }
            else
            {
                if (tagmodel.IsChanged)
                {
                    var re = DevelopServiceHelper.Helper.UpdateTag(GroupModel.Database, tagmodel.RealTagMode);
                    if (re)
                    {
                        tagmodel.IsChanged = false;
                    }
                    return re;
                }
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        public void UpdateAll()
        {
            foreach(var vv in this.mSelectGroupTags)
            {
                UpdateTag(vv);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Active()
        {
            mTotalPageNumber = -1;
            ContinueQueryTags();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DeActive()
        {
            UpdateAll();
        }


        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
