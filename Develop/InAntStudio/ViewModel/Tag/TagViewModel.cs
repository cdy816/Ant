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
        public static string[] mRecordTypeList;
        public static string[] mCompressTypeList;
        public static string[] mReadWriteModeList;

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, string[]> Drivers;

        private string mDriverName;
        private string mRegistorName;

        private string[] mRegistorList;

        private ICommand mConvertEditCommand;

        private ICommand mConvertRemoveCommand;

        private bool mIsSelected;

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
            mCompressTypeList = new string[] 
            {
                Res.Get("NoneCompress"),
                Res.Get("LosslessCompress"),
                Res.Get("DeadAreaCompress"),
                Res.Get("SlopeCompress")
            };
        }

        public TagViewModel()
        {

        }

        public TagViewModel(Cdy.Ant.Tagbase realTag)
        {
            this.mRealTagMode = realTag;
            CheckLinkAddress();
        }

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public bool IsChanged { get; set; }

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
                    OnPropertyChanged("IsNumberTag");
                    OnPropertyChanged("Precision");
                    OnPropertyChanged("MaxValue");
                    OnPropertyChanged("MinValue");
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


        #endregion ...Properties....

        #region ... Methods    ...

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
        public void RefreshHisTag()
        {
            OnPropertyChanged("CompressType");
            OnPropertyChanged("CompressCircle");
            OnPropertyChanged("RecordType");
            OnPropertyChanged("RecordTypeString");
            OnPropertyChanged("IsTimerRecord");
            OnPropertyChanged("IsDriverRecord");
            OnPropertyChanged("MaxValueCountPerSecond");
        }

        /// <summary>
        /// 
        /// </summary>
        private void CheckLinkAddress()
        {
            if (string.IsNullOrEmpty(LinkAddress))
            {
                return;
            }
            else
            {
                string[] str = LinkAddress.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                mDriverName = str[0];
                if(str.Length>1)
                {
                    mRegistorName = LinkAddress.Substring(mDriverName.Length+1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void InitEnumType()
        {
            mTagTypeList = Enum.GetNames(typeof(Cdy.Ant.TagType));
            //mRecordTypeList = Enum.GetNames(typeof(Cdy.Ant.RecordType)).Select(e => Res.Get(e)).ToArray();
            //mReadWriteModeList = Enum.GetNames(typeof(Cdy.Ant.ReadWriteMode)).Select(e=>Res.Get(e)).ToArray();
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
                    ntag = new Cdy.Ant.StringAlarmTag() { Id = this.mRealTagMode.Id, Name = mRealTagMode.Name, Desc = mRealTagMode.Desc, Group = mRealTagMode.Group };
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

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }



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
