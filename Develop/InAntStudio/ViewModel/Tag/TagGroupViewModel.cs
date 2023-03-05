//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/6/11 16:18:22.
//  Version 1.0
//  种道洋
//==============================================================


using Cdy.Ant;
using DBDevelopClientApi;
using Google.Protobuf.WellKnownTypes;
using InAntStudio.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class TagGroupViewModel : HasChildrenTreeItemViewModel
    {

        #region ... Variables  ...

        internal string mDescription;

        private ICommand mSetDescriptionCommand;

        private bool mIsDescriptionEdit = false;

        public static TagGroupViewModel CopyTarget { get; set; }
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        public TagGroupViewModel()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public TagGroupViewModel(string name, string description)
        {
            mDescription = description;
            Name = name;
        }
        #endregion ...Constructor...

        #region ... Properties ...

        public ICommand SetDescriptionCommand
        {
            get
            {
                if(mSetDescriptionCommand==null)
                {
                    mSetDescriptionCommand = new RelayCommand(() => {
                        IsDescriptionEdit = true;
                    });
                }
                return mSetDescriptionCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string FullName => (Parent!=null && !(Parent is RootTagGroupViewModel)) ? (Parent as TagGroupViewModel).FullName + "." + Name : Name;

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                if (mDescription != value)
                {
                    if (DevelopServiceHelper.Helper.UpdateGroupDescription(this.Database, this.FullName, value))
                    {
                        mDescription = value;
                        IsDescriptionEdit = false;
                    }
                    OnPropertyChanged("Description");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public bool IsDescriptionEdit
        {
            get
            {
                return mIsDescriptionEdit;
            }
            set
            {
                if (mIsDescriptionEdit != value)
                {

                    mIsDescriptionEdit = value;
                    OnPropertyChanged("IsDescriptionEdit");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ViewModelBase GetModel(ViewModelBase mode)
        {
            if(mode is TagGroupDetailViewModel)
            {
                (mode as TagGroupDetailViewModel).GroupModel = this;
                return mode;
            }
            else
            {
                return new TagGroupDetailViewModel() { GroupModel = this };
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanPaste()
        {
            return CopyTarget != null && !(this.FullName.Contains(CopyTarget.FullName) || (CopyTarget.FullName.Contains(this.FullName) && !string.IsNullOrEmpty(this.FullName)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanCopy()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddChild()
        {
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanRemove()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Remove()
        {
            string sname = this.FullName;
            if (MessageBox.Show(string.Format(Res.Get("RemoveConfirmMsg"), sname), "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (DevelopServiceHelper.Helper.RemoveTagGroup(Database, sname))
                {
                    if (Parent != null && Parent is HasChildrenTreeItemViewModel)
                    {
                        (Parent as HasChildrenTreeItemViewModel).Children.Remove(this);
                        Parent = null;
                    }
                    if (CopyTarget == this)
                        CopyTarget = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetNewGroupName()
        {
            List<string> vtmps = Children.Select(e => e.Name).ToList();
            string tagName = "group";
            for (int i = 1; i < int.MaxValue; i++)
            {
                tagName = "group" + i;
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
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool AddGroup(string parent)
        {
            string chileName = GetNewGroupName();
            chileName = DevelopServiceHelper.Helper.AddTagGroup(this.Database, chileName, parent);
            if (!string.IsNullOrEmpty(chileName))
            {
                var vmm = new TagGroupViewModel() { mName = chileName, Database = this.Database, Parent = this };
                this.Children.Add(vmm);
                
                if (!this.IsExpanded) this.IsExpanded = true;
                vmm.IsSelected = true;
                vmm.IsEdit = true;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Add()
        {
            AddGroup(this.FullName);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Copy()
        {
            CopyTarget = this;
        }


        /// <summary>
        /// 
        /// </summary>
        public override void Paste()
        {
            string sgroup = DevelopServiceHelper.Helper.PasteTagGroup(Database, CopyTarget.FullName, FullName);
            if (!string.IsNullOrEmpty(sgroup))
            {
                this.Children.Add(new TagGroupViewModel() { Database = this.Database, mName = sgroup,mDescription="" });
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public override bool OnRename(string oldName, string newName)
        {
            return DevelopServiceHelper.Helper.ReNameTagGroup(Database, this.FullName, newName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groups"></param>
        public void InitData(List<Tuple<string,string, string>> groups)
        {
            foreach (var vv in groups.Where(e => e.Item3 == this.FullName))
            {
                string sname = vv.Item1;
                if (!string.IsNullOrEmpty(vv.Item3))
                {
                    sname = sname.Substring(sname.IndexOf(vv.Item3) + vv.Item3.Length + 1);
                }
                TagGroupViewModel groupViewModel = new TagGroupViewModel() { mName = sname, Database = Database,mDescription=vv.Item2 };
                groupViewModel.Parent = this;
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
    public class RootTagGroupViewModel : TagGroupViewModel
    {

        #region ... Variables  ...
        private ICommand mImportFromMarsCommand;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        public RootTagGroupViewModel()
        {
            Name = Res.Get("Tag");
        }
        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public override string FullName => string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public ICommand ImportFromMarsCommand
        {
            get
            {
                if (mImportFromMarsCommand == null)
                {
                    mImportFromMarsCommand = new RelayCommand(() => {
                        ImportFromMars();
                    });
                }
                return mImportFromMarsCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MainViewModel Owner { get; set; }

        private bool mIsBusy = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return mIsBusy;
            }
            set
            {
                if (mIsBusy != value)
                {
                    mIsBusy = value;
                    OnPropertyChanged("IsBusy");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        private void ImportFromMars()
        {
            string database = ServiceHelper.Helper.Database;
            string saddr = ServiceHelper.Helper.Server;
            string pass = ServiceHelper.Helper.Password;
            string user = ServiceHelper.Helper.UserName;

            if (string.IsNullOrEmpty(database))
            {
                MarsSyncConfigViewModel viewModel = new MarsSyncConfigViewModel();
                if (viewModel.ShowDialog().Value)
                {
                    database = viewModel.CurrentDatabase;
                    saddr = viewModel.ServerAddress;
                    pass = viewModel.ServerPassword;
                    user = viewModel.ServerUserName;
                }
                else
                {
                    return;
                }
            }

            if (!saddr.Contains(":"))
            {
                saddr = saddr + ":9000";
            }

            var client = GetClient(saddr, user, pass);
            if(client==null)
            {
                MessageBox.Show(String.Format(Res.Get("ConnectServerErro"),saddr));
                return;
            }
            var antgroups = ListAllGroup();
            if(client!=null)
            {
                var grps = client.GetTagGroup(database);    
                if(grps!=null)
                {
                    foreach(var grp in grps)
                    {
                        string sgroupname = grp.Name;
                        if(!antgroups.Contains(sgroupname))
                        {
                            string sname = grp.Name;
                            if(!string.IsNullOrEmpty(grp.Parent))
                            {
                                sname = grp.Name.Substring(grp.Parent.Length+1);
                            }
                            AddGroupInner(grp.Parent, sname);
                        }
                        SyncTagGroup(database, sgroupname, client);
                    }

                    //同步根组
                    SyncTagGroup(database, "", client);

                    Task.Run(() => {
                        Owner?.QueryGroups();
                    });
                }

                ServiceLocator.Locator.Resolve<IRefreshContent>()?.RefreshContent();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool AddGroupInner(string parent,string group)
        {
            group = DevelopServiceHelper.Helper.AddTagGroup(this.Database, group, parent);
            if (!string.IsNullOrEmpty(group))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> ListAllGroup()
        {
            List<string> re = new List<string>();
            var vvs = DevelopServiceHelper.Helper.QueryTagGroups(this.Database);
            re.AddRange(vvs.Select(e => string.IsNullOrEmpty(e.Item3) ? e.Item1 : e.Item3 + "." + e.Item1));
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="group"></param>
        /// <param name="client"></param>
        private void SyncTagGroup(string database, string group, DBDevelopClientWebApi.DevelopServiceHelper client)
        {
            IsBusy = true;
            ServiceLocator.Locator.Resolve<IProcessNotify>().BeginShowNotify();
            Task.Run(() =>
            {
                try
                {
                    int currentpage = 0, pagecount = 10;
                    while (currentpage < pagecount)
                    {
                        var tags = client.GetTagByGroup(database, group, currentpage++, out pagecount);
                        ServiceLocator.Locator.Resolve<IProcessNotify>().ShowNotifyValue(currentpage * 100.0 / pagecount);
                        var atags = ListTags(group);
                        foreach (var vv in tags)
                        {
                            if (!atags.Contains(vv.Item1.Name))
                            {
                                var vtag = new MarsTagViewModel() { Name = vv.Item1.Name, Desc = vv.Item1.Desc, Type = vv.Item1.Type.ToString(), ReadWriteMode = vv.Item1.ReadWriteType.ToString(), Group = group };
                                if (vv.Item1 is Cdy.Tag.NumberTagBase)
                                {
                                    vtag.MaxValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MaxValue;
                                    vtag.MinValue = (vv.Item1 as Cdy.Tag.NumberTagBase).MinValue;
                                }
                                if (vv.Item1 is Cdy.Tag.FloatingTagBase)
                                {
                                    vtag.Precision = (vv.Item1 as Cdy.Tag.FloatingTagBase).Precision;
                                }

                                var tag = vtag.ConvertTo();
                                if (tag != null)
                                {
                                    tag.RealTagMode.Group = group;

                                    DevelopServiceHelper.Helper.AddTag(database, tag.RealTagMode, out int id);
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
                ServiceLocator.Locator.Resolve<IProcessNotify>().EndShowNotify();
                IsBusy = false;
                MessageBox.Show("同步完成!");
            });
        }

        private DBDevelopClientWebApi.DevelopServiceHelper GetClient(string addr,string user,string pass)
        {
            DBDevelopClientWebApi.DevelopServiceHelper mHelper = new DBDevelopClientWebApi.DevelopServiceHelper();
            mHelper.Server = addr;

            if (!mHelper.Server.StartsWith("http://"))
            {
                mHelper.Server = "http://" + mHelper.Server;
            }
            if (mHelper.Login(user, pass))
            {
               
                return mHelper;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<string> ListTags(string parent)
        {
            var tags = DevelopServiceHelper.Helper.QueryTagByGroup(this.Database, parent);
            if(tags!=null)
            {
                return tags.Values.Select(e=>e.Name).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanRemove()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanCopy()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public override bool OnRename(string oldName, string newName)
        {
            return true;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
