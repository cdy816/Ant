//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/1/2 10:04:50.
//  Version 1.0
//  种道洋
//==============================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Input;

using InAntStudio.ViewModel;
using Cdy.Ant;

namespace InAntStudio
{
    /// <summary>
    /// 
    /// </summary>
    public class TagViewModel : ViewModelBase
    {

        #region ... Variables  ...
        private Cdy.Ant.Tagbase mRealTagMode;

        public static string[] mTagTypeList;

        private bool mIsSelected;

        private TagDetailConfigViewModelBase mTagDetailModel;

        private bool mIsChanged = false;

        private ICommand mTagBrowseCommand;

        private ICommand mClearLinkTagsCommand;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        static TagViewModel()
        {
            InitEnumType();
        }

        public TagViewModel()
        {

        }

        public TagViewModel(Cdy.Ant.Tagbase realTag)
        {
            this.RealTagMode = realTag;
            //CheckLinkAddress();
        }

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public ICommand ClearLinkTagsCommand
        {
            get
            {
                if(mClearLinkTagsCommand==null)
                {
                    mClearLinkTagsCommand = new RelayCommand(() => {
                        LinkAddress = string.Empty;
                    });
                }
                return mClearLinkTagsCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand TagBrowseCommand
        {
            get
            {
                if(mTagBrowseCommand==null)
                {
                    mTagBrowseCommand = new RelayCommand(() => {
                        MarsTagBrowserViewModel mm = new MarsTagBrowserViewModel();
                        mm.ServerAddress = ServiceHelper.Helper.Server;
                        mm.ServerPassword = ServiceHelper.Helper.Password;
                        mm.ServerUserName = ServiceHelper.Helper.UserName;
                        mm.PreLoadDatabase = ServiceHelper.Helper.Database;
                        List<TagViewModel> ltmp = new List<TagViewModel>();
                        if (mm.ShowDialog().Value)
                        {
                            foreach (var vv in mm.GetSelectTags())
                            {
                                string sname = vv.FullName;
                                if (!string.IsNullOrEmpty(LinkAddress))
                                {
                                    if (!LinkAddress.Contains(sname))
                                    {
                                        LinkAddress += "," + sname;
                                    }
                                }
                                else
                                {
                                    LinkAddress = sname;
                                }
                            }
                        }
                    });
                }
                return mTagBrowseCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TagDetailConfigViewModelBase TagDetailModel
        {
            get
            {
                return mTagDetailModel;
            }
            set
            {
                if (mTagDetailModel != value)
                {
                    if (mTagDetailModel != null)
                    {
                        mTagDetailModel.PropertyChanged -= MTagDetailModel_PropertyChanged;
                    }
                    mTagDetailModel = value;
                    if(mTagDetailModel!=null)
                    {
                        mTagDetailModel.PropertyChanged += MTagDetailModel_PropertyChanged;
                    }
                    OnPropertyChanged("TagDetailModel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsChanged { get { return mIsChanged; } set { mIsChanged = value; if (!value && mTagDetailModel!=null) mTagDetailModel.IsChanged = false; } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSelected 
        { 
            get { return mIsSelected; } 
            set 
            { 
                mIsSelected = value; 
                OnPropertyChanged("IsSelected"); 
            }
        }

        /// <summary>
        /// 是否新建
        /// </summary>
        public bool IsNew { get; set; }


        /// <summary>
        /// 实时变量配置
        /// </summary>
        public Cdy.Ant.Tagbase RealTagMode
        {
            get
            {
                return mRealTagMode;
            }
            set
            {
                if (mRealTagMode != value)
                {
                    mRealTagMode = value;
                    InitTagDetailModel();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get
            {
                return mRealTagMode != null ? mRealTagMode.Id : -1;
            }
            set
            {
                if(mRealTagMode!=null)
                mRealTagMode.Id = value;
                OnPropertyChanged("Id");
            }
        }


        /// <summary>
        /// 名字
        /// </summary>
        public string Name
        {
            get
            {
                return mRealTagMode != null ? mRealTagMode.Name : string.Empty;
            }
            set
            {
                if (mRealTagMode != null && mRealTagMode.Name != value && CheckAvaiableName(value))
                {
                    mRealTagMode.Name = value;
                    IsChanged = true;
                }
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc
        {
            get
            {
                return mRealTagMode != null ? mRealTagMode.Desc : string.Empty;
            }
            set
            {
                if (mRealTagMode != null && mRealTagMode.Desc != value)
                {
                    mRealTagMode.Desc = value;
                    IsChanged = true;
                    OnPropertyChanged("Desc");
                }
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
        /// 类型
        /// </summary>
        public int Type
        {
            get
            {
                return mRealTagMode != null ? (int)mRealTagMode.Type : -1;
            }
            set
            {
                if (mRealTagMode != null && (int)mRealTagMode.Type != value)
                {
                    ChangeTagType((Cdy.Ant.TagType)value);
                    IsChanged = true;
                    OnPropertyChanged("Type");
                    OnPropertyChanged("TypeString");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TypeString
        {
            get
            {
                return mRealTagMode.Type.ToString();
            }
        }

        /// <summary>
        /// 组
        /// </summary>
        public string Group
        {
            get
            {
                return mRealTagMode != null ? mRealTagMode.Group : string.Empty;
            }
            set
            {
                if (mRealTagMode != null && mRealTagMode.Group != value)
                {
                    mRealTagMode.Group = value;
                    IsChanged = true;
                    OnPropertyChanged("Group");
                }
            }
        }

        /// <summary>
        /// 关联外部地址
        /// </summary>
        public string LinkAddress
        {
            get
            {
                return mRealTagMode != null && mRealTagMode is AlarmTag ? (mRealTagMode as AlarmTag).LinkTag : string.Empty;
            }
            set
            {
                if (mRealTagMode != null && mRealTagMode is AlarmTag && (mRealTagMode as AlarmTag).LinkTag != value)
                {
                    (mRealTagMode as AlarmTag).LinkTag = value;
                    IsChanged = true;
                    OnPropertyChanged("LinkAddress");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public bool IsEnable
        {
            get
            {
                return mRealTagMode.IsEnable;
            }
            set
            {
                if (mRealTagMode.IsEnable != value)
                {
                    mRealTagMode.IsEnable = value;
                    IsChanged = true;
                    OnPropertyChanged("IsEnable");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string CustomContent1
        {
            get
            {
                return mRealTagMode.CustomContent1;
            }
            set
            {
                if (mRealTagMode.CustomContent1 != value)
                {
                    mRealTagMode.CustomContent1 = value;
                    OnPropertyChanged("CustomContent1");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CustomContent2
        {
            get
            {
                return mRealTagMode.CustomContent2;
            }
            set
            {
                if (mRealTagMode.CustomContent2 != value)
                {
                    mRealTagMode.CustomContent2 = value;
                    OnPropertyChanged("CustomContent2");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CustomContent3
        {
            get
            {
                return mRealTagMode.CustomContent3;
            }
            set
            {
                if (mRealTagMode.CustomContent3 != value)
                {
                    mRealTagMode.CustomContent3 = value;
                    OnPropertyChanged("CustomContent3");
                }
            }
        }

        #endregion ...Properties....

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        private void InitTagDetailModel()
        {
            switch(mRealTagMode.Type)
            {
                case TagType.AnalogAlarm:
                    TagDetailModel = new AnalogTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.AnalogRangeAlarm:
                    TagDetailModel = new AnalogRangTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.DelayDigitalAlarm:
                    TagDetailModel = new DigitalDelayTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.DigitalAlarm:
                    TagDetailModel = new DigitalTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.OneRange:
                    TagDetailModel = new OneRangeTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.Pulse:
                    TagDetailModel = new PulseTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.Script:
                    TagDetailModel = new ScriptTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.StringAlarm:
                    TagDetailModel = new StringTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.ThreeRange:
                    TagDetailModel = new ThreeRangeTagConfigViewModel() { Model = mRealTagMode };
                    break;
                case TagType.TwoRange:
                    TagDetailModel = new TwoRangeTagConfigViewModel() { Model = mRealTagMode };
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MTagDetailModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChanged")
            {
                if (mTagDetailModel.IsChanged)
                {
                    IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckAvaiableName(string name)
        {
            return !name.Contains(".");
        }

        /// <summary>
        /// 
        /// </summary>
        private static void InitEnumType()
        {
            mTagTypeList = Enum.GetNames(typeof(Cdy.Ant.TagType));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagType"></param>
        private void ChangeTagType(Cdy.Ant.TagType tagType)
        {
            Cdy.Ant.Tagbase ntag = null;
            switch (tagType)
            {
                case Cdy.Ant.TagType.AnalogAlarm:
                    ntag = new Cdy.Ant.AnalogAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.AnalogRangeAlarm:
                    ntag = new Cdy.Ant.AnalogRangeAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc,  Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.DigitalAlarm:
                    ntag = new Cdy.Ant.DigitalAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
                    
                    break;
                case Cdy.Ant.TagType.DelayDigitalAlarm:
                    ntag = new Cdy.Ant.DelayDigitalAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.OneRange:
                    ntag = new Cdy.Ant.OneRangeAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.Pulse:
                    ntag = new Cdy.Ant.PulseAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc,  Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.Script:
                    ntag = new Cdy.Ant.ScriptTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.StringAlarm:
                    ntag = new Cdy.Ant.StringAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.ThreeRange:
                    ntag = new Cdy.Ant.ThreeRangeAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc,  Group = mRealTagMode.Group };
                    break;
                case Cdy.Ant.TagType.TwoRange:
                    ntag = new Cdy.Ant.TwoRangeAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
                    break;
                

                default:
                    break;
            }
            if (ntag != null)
            {
                if((RealTagMode is Cdy.Ant.AlarmTag) && (ntag is Cdy.Ant.AlarmTag))
                {
                    (ntag as AlarmTag).LinkTag = (RealTagMode as AlarmTag).LinkTag;
                }
                RealTagMode = ntag;
            }
            InitTagDetailModel();
            IsChanged = true;
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TagViewModel Clone()
        {
            Cdy.Ant.Tagbase ntag = mRealTagMode.Clone();
            return new TagViewModel(ntag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string SaveToCSVString()
        {
            return mRealTagMode.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public static TagViewModel LoadFromCSVString(string val)
        {
            string[] stmp = val.Split(new char[] { ',' });
            var re = TagManager.Manager.CreatTag((Cdy.Ant.TagType)Enum.Parse(typeof(Cdy.Ant.TagType), stmp[0]));
            re.LoadFromString(val);
            return new TagViewModel(re);
        }
       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static TagViewModel LoadFromMarsCSVString(string val)
        {
            string[] stmp = val.Split(new char[] { ',' });
            MarsTagType tp = (MarsTagType)Enum.Parse(typeof(MarsTagType), stmp[4]);

            string sname = stmp[1];
            string group = stmp[3];
            string fullname = string.IsNullOrEmpty(group) ? sname : group + "." + sname;
            string desc = stmp[2];

            Tagbase tb=null;
            switch (tp)
            {
                case MarsTagType.Bool:
                    tb = new Cdy.Ant.DigitalAlarmTag();
                    break;
                case MarsTagType.Byte:
                case MarsTagType.Short:
                case MarsTagType.UShort:
                case MarsTagType.Int:
                case MarsTagType.Long:
                case MarsTagType.UInt:
                case MarsTagType.ULong:
                case MarsTagType.Float:
                case MarsTagType.Double:
                    tb = new Cdy.Ant.AnalogAlarmTag();
                    break;
                case MarsTagType.String:
                    tb = new Cdy.Ant.StringAlarmTag();
                    break;
            }

            if (tb != null)
            {
                tb.Name = sname;
                tb.Desc = desc;
                if(tb is AlarmTag)
                {
                    (tb as AlarmTag).LinkTag = fullname;
                }
                return new TagViewModel(tb);
            }
            return null;

        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract  class TagDetailConfigViewModelBase : ViewModelBase
    {

        #region ... Variables  ...
        private Cdy.Ant.Tagbase mModel;
        private bool mIsChanged = false;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public Cdy.Ant.Tagbase Model
        {
            get
            {
                return mModel;
            }
            set
            {
                if (mModel != value)
                {
                    mModel = value;
                    Init();
                    OnPropertyChanged("Model");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public bool IsChanged
        {
            get
            {
                return mIsChanged;
            }
            set
            {
                if (mIsChanged != value)
                {
                    mIsChanged = value;
                    OnPropertyChanged("IsChanged");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        protected virtual void Init()
        {

        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    public abstract class AlarmTagConfigViewModel : TagDetailConfigViewModelBase
    {

        #region ... Variables  ...
        Cdy.Ant.AlarmTag mTag;
        private ICommand mTagBrowerCommand;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public string LinkTag
        {
            get
            {
                return mTag.LinkTag;
            }
            set
            {
                if (mTag.LinkTag != value)
                {
                    mTag.LinkTag = value;
                    OnPropertyChanged("LinkTag");
                    IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand TagBrowerCommand
        {
            get
            {
                if(mTagBrowerCommand==null)
                {
                    mTagBrowerCommand = new RelayCommand(() => { 
                    
                    });
                }
                return mTagBrowerCommand;
            }
        }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            mTag = Model as AlarmTag;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    public enum MarsTagType
    {
        /// <summary>
        /// 
        /// </summary>
        Bool,
        /// <summary>
        /// 
        /// </summary>
        Byte,
        /// <summary>
        /// 
        /// </summary>
        Short,
        /// <summary>
        /// 
        /// </summary>
        UShort,
        /// <summary>
        /// 
        /// </summary>
        Int,
        /// <summary>
        /// 
        /// </summary>
        UInt,
        /// <summary>
        /// 
        /// </summary>
        Long,
        /// <summary>
        /// 
        /// </summary>
        ULong,
        /// <summary>
        /// 
        /// </summary>
        Double,
        /// <summary>
        /// 
        /// </summary>
        Float,
        /// <summary>
        /// 
        /// </summary>
        DateTime,
        /// <summary>
        /// 
        /// </summary>
        String,
        /// <summary>
        /// 
        /// </summary>
        IntPoint,
        /// <summary>
        /// 
        /// </summary>
        UIntPoint,
        /// <summary>
        /// 
        /// </summary>
        LongPoint,
        /// <summary>
        /// 
        /// </summary>
        ULongPoint,
        /// <summary>
        /// 
        /// </summary>
        IntPoint3,
        /// <summary>
        /// 
        /// </summary>
        UIntPoint3,
        /// <summary>
        /// 
        /// </summary>
        LongPoint3,
        /// <summary>
        /// 
        /// </summary>
        ULongPoint3
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class SimpleTagConfigViewModel:AlarmTagConfigViewModel
    {

        #region ... Variables  ...
        private Cdy.Ant.SimpleAlarmTag mTag;
        private static string[] mAlarmLevels;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// 
        /// </summary>
        static SimpleTagConfigViewModel()
        {
            InitEnumType();
        }
        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public string[] AlarmLevels
        {
            get
            {
                return mAlarmLevels;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int AlarmLevel
        {
            get
            {
                return (byte)mTag.AlarmLevel;
            }
            set
            {
                if ((byte)mTag.AlarmLevel != value)
                {
                    mTag.AlarmLevel = (Cdy.Ant.AlarmLevel) value;
                    OnPropertyChanged("AlarmLevel");
                    IsChanged = true;
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        private static void InitEnumType()
        {
            mAlarmLevels = Enum.GetNames(typeof(Cdy.Ant.AlarmLevel));
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.SimpleAlarmTag;
        }



        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnalogTagConfigViewModel : AlarmTagConfigViewModel
    {

        #region ... Variables  ...
        private Cdy.Ant.AnalogAlarmTag mTag;
        private static string[] mAlarmLevels;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        static AnalogTagConfigViewModel()
        {
            InitEnumType();
        }
        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public string[] AlarmLevels
        {
            get
            {
                return mAlarmLevels;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasHighHighValue
        {
            get
            {
                return mTag.HighHighValue!=null;
            }
            set
            {
                if (value && mTag.HighHighValue == null)
                {
                    mTag.HighHighValue = new AnalogAlarmItem() { AlarmLevel = AlarmLevel.Urgency };
                    IsChanged = true;
                }
                else if (!value)
                {
                    mTag.HighHighValue = null;
                }
                OnPropertyChanged("HasHighHighValue");
                OnPropertyChanged("HighHighAlarmLevel");
                OnPropertyChanged("HighHighValue");
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public int HighHighAlarmLevel
        {
            get
            {
                return mTag.HighHighValue!=null? (int) mTag.HighHighValue.AlarmLevel:0;
            }
            set
            {
                
                if ((byte)mTag.HighHighValue.AlarmLevel != value)
                {
                    mTag.HighHighValue.AlarmLevel = (AlarmLevel)value;
                    IsChanged = true;
                    OnPropertyChanged("HighHighAlarmLevel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double HighHighValue
        {
            get
            {
                return mTag.HighHighValue != null ? mTag.HighHighValue.Value : 0;
            }
            set
            {

                if (mTag.HighHighValue.Value != value)
                {
                    mTag.HighHighValue.Value = value;
                    IsChanged = true;
                    OnPropertyChanged("HighHighValue");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double HighHighDeadArea
        {
            get
            {
                return mTag.HighHighValue != null ? mTag.HighHighValue.DeadArea : 0;
            }
            set
            {

                if (mTag.HighHighValue.DeadArea != value)
                {
                    mTag.HighHighValue.DeadArea = value;
                    IsChanged = true;
                    OnPropertyChanged("HighHighDeadArea");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasHighValue
        {
            get
            {
                return mTag.HighValue != null;
            }
            set
            {
                if (value && mTag.HighValue == null)
                {
                    mTag.HighValue = new AnalogAlarmItem() { AlarmLevel = AlarmLevel.Critical };
                    IsChanged = true;
                    
                }
                else if(!value)
                {
                    mTag.HighValue = null;
                }
                OnPropertyChanged("HasHighValue");
                OnPropertyChanged("HighAlarmLevel");
                OnPropertyChanged("HighValue");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int HighAlarmLevel
        {
            get
            {
                return mTag.HighValue != null ? (int)mTag.HighValue.AlarmLevel : 0;
            }
            set
            {

                if ((byte)mTag.HighValue.AlarmLevel != value)
                {
                    mTag.HighValue.AlarmLevel = (AlarmLevel)value;
                    IsChanged = true;
                    OnPropertyChanged("HighAlarmLevel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double HighValue
        {
            get
            {
                return mTag.HighValue != null ? mTag.HighValue.Value : 0;
            }
            set
            {

                if (mTag.HighValue.Value != value)
                {
                    mTag.HighValue.Value = value;
                    IsChanged = true;
                    OnPropertyChanged("HighValue");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double HighDeadArea
        {
            get
            {
                return mTag.HighValue != null ? mTag.HighValue.DeadArea : 0;
            }
            set
            {

                if (mTag.HighValue.DeadArea != value)
                {
                    mTag.HighValue.DeadArea = value;
                    IsChanged = true;
                    OnPropertyChanged("HighDeadArea");
                }
            }
        }

        public bool HasLowValue
        {
            get
            {
                return mTag.LowValue != null;
            }
            set
            {
                if (value && mTag.LowValue == null)
                {
                    mTag.LowValue = new AnalogAlarmItem() { AlarmLevel = AlarmLevel.Critical };
                    IsChanged = true;
                }
                else if(!value)
                {
                    mTag.LowValue = null;
                }
                OnPropertyChanged("HasLowValue");
                OnPropertyChanged("LowAlarmLevel");
                OnPropertyChanged("LowValue");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int LowAlarmLevel
        {
            get
            {
                return mTag.LowValue != null ? (int)mTag.LowValue.AlarmLevel : 0;
            }
            set
            {

                if ((byte)mTag.LowValue.AlarmLevel != value)
                {
                    mTag.LowValue.AlarmLevel = (AlarmLevel)value;
                    IsChanged = true;
                    OnPropertyChanged("LowAlarmLevel");
                }
            }
        }

        public double LowValue
        {
            get
            {
                return mTag.LowValue != null ? mTag.LowValue.Value : 0;
            }
            set
            {

                if (mTag.LowValue.Value != value)
                {
                    mTag.LowValue.Value = value;
                    IsChanged = true;
                    OnPropertyChanged("LowValue");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double LowDeadArea
        {
            get
            {
                return mTag.LowValue != null ? mTag.LowValue.DeadArea : 0;
            }
            set
            {

                if (mTag.LowValue.DeadArea != value)
                {
                    mTag.LowValue.DeadArea = value;
                    IsChanged = true;
                    OnPropertyChanged("LowDeadArea");
                }
            }
        }


        public bool HasLowLowValue
        {
            get
            {
                return mTag.LowLowValue != null;
            }
            set
            {
                if (value && mTag.LowLowValue == null)
                {
                    mTag.LowLowValue = new AnalogAlarmItem() { AlarmLevel = AlarmLevel.Urgency };
                    IsChanged = true;
                    
                }
                else if (!value)
                {
                    mTag.LowLowValue = null;
                }
                OnPropertyChanged("HasLowLowValue");
                OnPropertyChanged("LowLowAlarmLevel");
                OnPropertyChanged("LowLowValue");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int LowLowAlarmLevel
        {
            get
            {
                return mTag.LowLowValue != null ? (int)mTag.LowLowValue.AlarmLevel : 0;
            }
            set
            {

                if ((byte)mTag.LowLowValue.AlarmLevel != value)
                {
                    mTag.LowLowValue.AlarmLevel = (AlarmLevel)value;
                    IsChanged = true;
                    OnPropertyChanged("LowLowAlarmLevel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double LowLowValue
        {
            get
            {
                return mTag.LowLowValue != null ? mTag.LowLowValue.Value : 0;
            }
            set
            {

                if (mTag.LowLowValue.Value != value)
                {
                    mTag.LowLowValue.Value = value;
                    IsChanged = true;
                    OnPropertyChanged("LowLowValue");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double LowLowDeadArea
        {
            get
            {
                return mTag.LowLowValue != null ? mTag.LowLowValue.DeadArea : 0;
            }
            set
            {

                if (mTag.LowLowValue.DeadArea != value)
                {
                    mTag.LowLowValue.DeadArea = value;
                    IsChanged = true;
                    OnPropertyChanged("LowLowDeadArea");
                }
            }
        }

        #endregion ...Properties...

        #region ... Methods    ...

        private static void InitEnumType()
        {
            mAlarmLevels = Enum.GetNames(typeof(Cdy.Ant.AlarmLevel));
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.AnalogAlarmTag;
            
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnalogRangTagConfigViewModel:AlarmTagConfigViewModel
    {

        #region ... Variables  ...
        private Cdy.Ant.AnalogRangeAlarmTag mTag;
        private System.Collections.ObjectModel.ObservableCollection<AnalogRangTagItemConfigViewModel> mItems = new System.Collections.ObjectModel.ObservableCollection<AnalogRangTagItemConfigViewModel>();
        private ICommand mAddItemCommand;
        private ICommand mRemoveItemCommand;

        private AnalogRangTagItemConfigViewModel mCurrentItem;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public AnalogRangTagItemConfigViewModel CurrentItem
        {
            get
            {
                return mCurrentItem;
            }
            set
            {
                if (mCurrentItem != value)
                {
                    mCurrentItem = value;
                    OnPropertyChanged("CurrentItem");
                }
            }
        }


        public ICommand AddItemCommand
        {
            get
            {
                if(mAddItemCommand==null)
                {
                    mAddItemCommand = new RelayCommand(() => {
                        AddItem(new AnalogRangeAlarmItem() { MinValue = 0, MaxValue = 100 ,AlarmLevel = AlarmLevel.Normal,DeadArea=0});
                        IsChanged = true;
                    });
                }
                return mAddItemCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand RemoveItemCommand
        {
            get
            {
                if(mRemoveItemCommand==null)
                {
                    mRemoveItemCommand = new RelayCommand(() => { 
                        if(CurrentItem!=null)
                        {
                            CurrentItem.PropertyChanged += Mm_PropertyChanged;
                            Items.Remove(CurrentItem);
                        }
                    },()=> { return CurrentItem != null; });
                }
                return mRemoveItemCommand;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<AnalogRangTagItemConfigViewModel> Items
        {
            get
            {
                return mItems;
            }
        }
        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void AddItem(Cdy.Ant.AnalogRangeAlarmItem item)
        {
            AnalogRangTagItemConfigViewModel mm = new AnalogRangTagItemConfigViewModel() { Model = item };
            mm.PropertyChanged += Mm_PropertyChanged;
            mItems.Add(mm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsChanged = true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.AnalogRangeAlarmTag;
            if (mTag.Items != null)
            {
                foreach (var vv in mTag.Items)
                {
                    AddItem(vv);
                }
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class AnalogRangTagItemConfigViewModel : ViewModelBase
    {

        #region ... Variables  ...
        private Cdy.Ant.AnalogRangeAlarmItem mModel;
        private static string[] mAlarmLevels;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        static AnalogRangTagItemConfigViewModel()
        {
            InitEnumType();
        }
        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public string[] AlarmLevels
        {
            get
            {
                return mAlarmLevels;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Cdy.Ant.AnalogRangeAlarmItem Model
        {
            get
            {
                return mModel;
            }
            set
            {
                if (mModel != value)
                {
                    mModel = value;
                    OnPropertyChanged("Model");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public int AlarmLevel
        {
            get
            {
                return (byte)mModel.AlarmLevel;
            }
            set
            {
                if ((int)mModel.AlarmLevel != value)
                {
                    mModel.AlarmLevel = (Cdy.Ant.AlarmLevel) value;
                    OnPropertyChanged("AlarmLevel");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public double MaxValue
        {
            get
            {
                return mModel.MaxValue;
            }
            set
            {
                if (mModel.MaxValue != value)
                {
                    mModel.MaxValue = value;
                    OnPropertyChanged("MaxValue");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public double MinValue
        {
            get
            {
                return mModel.MinValue;
            }
            set
            {
                if (mModel.MinValue != value)
                {
                    mModel.MinValue = value;
                    OnPropertyChanged("MinValue");
                }
            }
        }


        /// <summary>
            /// 
            /// </summary>
        public double DeadArea
        {
            get
            {
                return mModel.DeadArea;
            }
            set
            {
                if (mModel.DeadArea != value)
                {
                    mModel.DeadArea = value;
                    OnPropertyChanged("DeadArea");
                }
            }
        }



        #endregion ...Properties...

        #region ... Methods    ...
        private static void InitEnumType()
        {
            mAlarmLevels = Enum.GetNames(typeof(Cdy.Ant.AlarmLevel));
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class DigitalTagConfigViewModel:SimpleTagConfigViewModel
    {

        #region ... Variables  ...
        Cdy.Ant.DigitalAlarmTag mTag;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public bool[] Values
        {
            get
            {
                return new bool[] { true, false };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Value
        {
            get
            {
                return mTag.Value;
            }
            set
            {
                if (mTag.Value != value)
                {
                    mTag.Value = value;
                    IsChanged = true;
                    OnPropertyChanged("Value");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.DigitalAlarmTag;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class DigitalDelayTagConfigViewModel:DigitalTagConfigViewModel
    {

        #region ... Variables  ...
        Cdy.Ant.DelayDigitalAlarmTag mTag;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public double Delay
        {
            get
            {
                return mTag.Delay;
            }
            set
            {
                if (mTag.Delay != value)
                {
                    mTag.Delay = value;
                    IsChanged = true;
                    OnPropertyChanged("Delay");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool Value { get => mTag.Value; set { mTag.Value = value; OnPropertyChanged("Value"); } }


        #endregion ...Properties...

        #region ... Methods    ...



        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.DelayDigitalAlarmTag;
            
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class PulseTagConfigViewModel : SimpleTagConfigViewModel
    {

        #region ... Variables  ...
        Cdy.Ant.PulseAlarmTag mTag;
        static string[] mPulseTypes;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        static PulseTagConfigViewModel()
        {

        }
        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public string[] PulseTypes
        {
            get
            {
                return mPulseTypes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Type
        {
            get
            {
                return (int)mTag.PulseType;
            }
            set
            {
                if ((int)mTag.PulseType != value)
                {
                    mTag.PulseType = (PulseAlarmType)value;
                    IsChanged = true;
                    OnPropertyChanged("Type");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        private static void InitEnumType()
        {
            mPulseTypes = Enum.GetNames(typeof(Cdy.Ant.PulseAlarmType));
        }

        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.PulseAlarmTag;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class OneRangeTagConfigViewModel : SimpleTagConfigViewModel
    {

        #region ... Variables  ...
        Cdy.Ant.OneRangeAlarmTag mTag;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
            /// 
            /// </summary>
        public double MaxValue
        {
            get
            {
                return mTag.MaxValue;
            }
            set
            {
                if (mTag.MaxValue != value)
                {
                    mTag.MaxValue = value;
                    IsChanged = true;
                    OnPropertyChanged("MaxValue");
                }
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public double MinValue
        {
            get
            {
                return mTag.MinValue;
            }
            set
            {
                if (mTag.MinValue != value)
                {
                    mTag.MinValue = value;
                    IsChanged = true;
                    OnPropertyChanged("MinValue");
                }
            }
        }


        /// <summary>
            /// 
            /// </summary>
        public bool IsInRangeAlarm
        {
            get
            {
                return mTag.IsInRangeAlarm;
            }
            set
            {
                if (mTag.IsInRangeAlarm != value)
                {
                    mTag.IsInRangeAlarm = value;
                    OnPropertyChanged("IsInRangeAlarm");
                    IsChanged = true;
                }
            }
        }



        #endregion ...Properties...

        #region ... Methods    ...
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.OneRangeAlarmTag;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class StringTagConfigViewModel : SimpleTagConfigViewModel
    {

        #region ... Variables  ...
        Cdy.Ant.StringAlarmTag mTag;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
            /// 
            /// </summary>
        public string Value
        {
            get
            {
                return mTag.Value;
            }
            set
            {
                if (mTag.Value != value)
                {
                    mTag.Value = value;
                    IsChanged = true;
                    OnPropertyChanged("Value");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.StringAlarmTag;
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class TwoRangeTagConfigViewModel : SimpleTagConfigViewModel
    {

        #region ... Variables  ...
        Cdy.Ant.TwoRangeAlarmTag mTag;
        private string mValue;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    mTag.AlarmDatas = DeSeriseAlarmPoint(value);
                    IsChanged = true;
                    OnPropertyChanged("Value");
                }
            }
        }

        public bool IsInRangeAlarm
        {
            get
            {
                return mTag.IsInRangeAlarm;
            }
            set
            {
                if (mTag.IsInRangeAlarm != value)
                {
                    mTag.IsInRangeAlarm = value;
                    OnPropertyChanged("IsInRangeAlarm");
                    IsChanged = true;
                }
            }
        }
        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public string ToStringAlarmPoint(List<Point> points)
        {
            StringBuilder sb = new StringBuilder();
            if(points!=null)
            foreach (var vv in points)
            {
                sb.Append(vv.X + "," + vv.Y + ";");
            }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<Point> DeSeriseAlarmPoint(string value)
        {
            var AlarmDatas = new List<Point>();
            string[] ss = value.Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { ',' });
                    double val1 = double.Parse(s1[0]);
                    double val2 = double.Parse(s1[1]);
                    AlarmDatas.Add(new Point() { X = val1, Y = val2 });
                }
            }
            return AlarmDatas;
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.TwoRangeAlarmTag;
            mValue = ToStringAlarmPoint(mTag.AlarmDatas);
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class ThreeRangeTagConfigViewModel : SimpleTagConfigViewModel
    {

        #region ... Variables  ...
        Cdy.Ant.ThreeRangeAlarmTag mTag;
        private string mValue;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    mTag.AlarmDatas = DeSeriseAlarmPoint(value);
                    IsChanged = true;
                    OnPropertyChanged("Value");
                }
            }
        }

        public bool IsInRangeAlarm
        {
            get
            {
                return mTag.IsInRangeAlarm;
            }
            set
            {
                if (mTag.IsInRangeAlarm != value)
                {
                    mTag.IsInRangeAlarm = value;
                    OnPropertyChanged("IsInRangeAlarm");
                    IsChanged = true;
                }
            }
        }
        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public string ToStringAlarmPoint(List<Point3D> points)
        {
            StringBuilder sb = new StringBuilder();
            if (points != null)
                foreach (var vv in points)
                {
                    sb.Append(vv.X + "," + vv.Y + "," + vv.Z + ";");
                }
            sb.Length = sb.Length > 0 ? sb.Length - 1 : sb.Length;
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public List<Point3D> DeSeriseAlarmPoint(string value)
        {
            var AlarmDatas = new List<Point3D>();
            string[] ss = value.Split(new char[] { ';' });
            if (ss.Length > 0)
            {
                foreach (var vv in ss)
                {
                    string[] s1 = vv.Split(new char[] { ',' });
                    double val1 = double.Parse(s1[0]);
                    double val2 = double.Parse(s1[1]);
                    double val3 = double.Parse(s1[2]);
                    AlarmDatas.Add(new Point3D() { X = val1, Y = val2,Z=val3 });
                }
            }
            return AlarmDatas;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.ThreeRangeAlarmTag;
            mValue = ToStringAlarmPoint(mTag.AlarmDatas);
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScriptTagConfigViewModel : TagDetailConfigViewModelBase
    {

        #region ... Variables  ...
        private Cdy.Ant.ScriptTag mTag;
        private ICommand mExpressEditCommand;
        private ICommand mExpressClearCommand;
        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public string Express
        {
            get
            {
                return mTag.Expresse;
            }
            set
            {
                if (mTag.Expresse != value)
                {
                    mTag.Expresse = value;
                    IsChanged = true;
                    OnPropertyChanged("Express");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        public ICommand ExpressEditCommand
        {
            get
            {
                if(mExpressEditCommand==null)
                {
                    mExpressEditCommand = new RelayCommand(() => {
                        ExpressionEditViewModel mm = new ExpressionEditViewModel();
                        mm.Expresse = this.Express;
                        if (mm.ShowDialog().Value)
                        {
                            Express = mm.GetExpressResult();
                        }
                    });
                }
                return mExpressEditCommand;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ExpressClearCommand
        {
            get
            {
                if(mExpressClearCommand==null)
                {
                    mExpressClearCommand = new RelayCommand(() => {
                        Express = string.Empty;
                    });
                }
                return mExpressClearCommand;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void Init()
        {
            base.Init();
            mTag = Model as Cdy.Ant.ScriptTag;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }


    /// <summary>
    /// 
    /// </summary>
    public class DatabaseViewModel : HasChildrenTreeItemViewModel
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
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public override bool OnRename(string oldName, string newName)
        {
            return base.OnRename(oldName, newName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddChild()
        {
            return false;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }


}
