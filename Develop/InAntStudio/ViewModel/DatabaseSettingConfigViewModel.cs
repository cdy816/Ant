//==============================================================
//  Copyright (C) 2020 Chongdaoyang Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/9/15 21:23:53 .
//  Version 1.0
//  CDYWORK
//==============================================================


using Cdy.Ant;
using Cdy.Ant.Tag;
using DBDevelopClientApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace InAntStudio.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseSettingConfigViewModel : ViewModelBase, IModeSwitch
    {

        #region ... Variables  ...
        private int mServerPort = 0;

        private ObservableCollection<DriverSetViewModel> mChildren = new ObservableCollection<DriverSetViewModel>();

        private string mApiKey;

        private string mProxyKey;

        private Setting mSetting;

        private IDataTagApiDevelop mApiConfig;

        private object mApiConfigModel;

        private IMessageServiceProxyDevelop mMessageConfig;

        private object mMessageConfigModel;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public List<string> ApiKeys
        {
            get
            {
                return ApiFactory.Factory.ListDevelopApis();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> ProxyKeys
        {
            get
            {
                return ProxyServiceFactory.Factory.ListDevelopApis();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ApiKey
        {
            get
            {
                return mApiKey;
            }
            set
            {
                if (mApiKey != value && value!=null)
                {
                    
                    mApiConfig = ApiFactory.Factory.GetDevelopInstance(value);

                    if (string.IsNullOrEmpty(mApiKey) && mApiConfig != null)
                    {
                        mApiConfig.Load(mSetting.ApiData);

                        ApiConfigModel = mApiConfig.Config();
                    }
                    mApiKey = value;
                    
                }
                OnPropertyChanged("ApiKey");
            }
        }

        /// <summary>
            /// 
            /// </summary>
        public string ProxyKey
        {
            get
            {
                return mProxyKey;
            }
            set
            {
                if (mProxyKey != value && value != null)
                {
                
                    mMessageConfig = ProxyServiceFactory.Factory.GetDevelopInstance(value);
                    if (string.IsNullOrEmpty(mProxyKey) && mMessageConfig != null)
                    {
                        mMessageConfig.Load(mSetting.ProxyData);
                        MessageConfigModel = mMessageConfig.Config();
                    }
                    else
                    {
                        MessageConfigModel = mMessageConfig.Config();
                    }
                    mProxyKey = value;
                    
                }
                OnPropertyChanged("ProxyKey");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public object ApiConfigModel
        {
            get
            {
                return mApiConfigModel;
            }
            set
            {
                if (mApiConfigModel != value)
                {
                    mApiConfigModel = value;
                    OnPropertyChanged("ApiConfigModel");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object MessageConfigModel
        {
            get
            {
                return mMessageConfigModel;
            }
            set
            {
                if (mMessageConfigModel != value)
                {
                    mMessageConfigModel = value;
                    OnPropertyChanged("MessageConfigModel");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public string Database { get; set; }

       

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            mSetting = DevelopServiceHelper.Helper.GetServerSetting(this.Database);
            mApiKey = mSetting.ApiType;
            mApiConfig = ApiFactory.Factory.GetDevelopInstance(mApiKey);

            if (mApiConfig != null)
            {
                mApiConfig.Load(mSetting.ApiData);

                ApiConfigModel = mApiConfig.Config();
            }

            mProxyKey = mSetting.ProxyType;

            mMessageConfig = ProxyServiceFactory.Factory.GetDevelopInstance(mProxyKey);
            if (mMessageConfig != null)
            {
                mMessageConfig.Load(mSetting.ProxyData);
                MessageConfigModel = mMessageConfig.Config();
            }

            OnPropertyChanged("ServerPort");
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...

        /// <summary>
        /// 
        /// </summary>
        public void Active()
        {
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DeActive()
        {
            if(ApiConfigModel!=null)
            {
                mSetting.ApiData = mApiConfig.Save();
                mSetting.ApiType = ApiKey;
            }
            if(MessageConfigModel!=null)
            {
                mSetting.ProxyData = mMessageConfig.Save();
                mSetting.ProxyType = ProxyKey;
            }
            DevelopServiceHelper.Helper.SetServerSetting(this.Database,mSetting);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DriverSetViewModel : ViewModelBase
    {

        #region ... Variables  ...

        private string mName;

        private ObservableCollection<DriverSettingItem> mChildren = new ObservableCollection<DriverSettingItem>();

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<DriverSettingItem> Children
        {
            get
            {
                return mChildren;
            }
        }

        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vals"></param>
        public void Init(Dictionary<string, string> vals)
        {
            mChildren.Clear();
            foreach (var vv in vals)
            {
                mChildren.Add(new DriverSettingItem() { Name = vv.Key, Value = vv.Value });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dtmp = new Dictionary<string, string>();
            foreach (var vv in mChildren)
            {
                dtmp.Add(vv.Name, vv.Value);
            }
            return dtmp;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    public class DriverSettingItem : ViewModelBase
    {

        #region ... Variables  ...
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
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName
        {
            get
            {
                return Res.Get(Name)+":";
            }
        }


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
                    OnPropertyChanged("Value");
                }
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
