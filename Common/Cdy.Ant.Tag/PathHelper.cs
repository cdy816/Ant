﻿//==============================================================
//  Copyright (C) 2021  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2021/8/18 23:45:02.
//  Version 1.0
//  种道洋
//==============================================================
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public class PathHelper
    {

        #region ... Variables  ...

        private string mAppPath;

        private string mDataPath;

        /// <summary>
        /// 
        /// </summary>
        public static PathHelper helper = new PathHelper();

        //private string mDatabaseName;
        private string mBasePath = "";

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        /// <summary>
        /// 
        /// </summary>
        public PathHelper()
        {
            mAppPath = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
            mBasePath = mAppPath;
            LoadBasePathConfig();
            mDataPath = System.IO.Path.Combine(mBasePath, "Data");
            //mDataPath = System.IO.Path.Combine(mAppPath,"Data");
        }

        #endregion ...Constructor...

        #region ... Properties ...
        
        /// <summary>
        /// 
        /// </summary>
        public string AppPath
        {
            get
            {
                return mAppPath;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DataPath
        {
            get
            {
                return mDataPath;
            }
            set
            {
                mDataPath = value;
            }
        }


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsWorkWithMars()
        {
            string sfile = System.IO.Path.Combine(System.IO.Directory.GetParent(AppPath).FullName, "DBInRun.dll");
            return System.IO.File.Exists(sfile);
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadBasePathConfig()
        {
            if (IsWorkWithMars())
            {
                mBasePath = System.IO.Directory.GetParent(AppPath).FullName;
                string sfile =  System.IO.Path.Combine(mBasePath, "BasePath.cfg");
                if(System.IO.File.Exists(sfile))
                {
                    string txt = System.IO.File.ReadAllText(sfile, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(txt) && System.IO.Directory.Exists(txt))
                    {
                        mBasePath = txt;
                    }
                }
            }
            else
            {
                string sfile = System.IO.Path.Combine(mAppPath, "BasePath.cfg");
                if (System.IO.File.Exists(sfile))
                {
                    string txt = System.IO.File.ReadAllText(sfile, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(txt) && System.IO.Directory.Exists(txt))
                    {
                        mBasePath = txt;
                    }
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="path"></param>
        //public void SetDataBasePath(string path)
        //{
        //    if (System.IO.Path.IsPathRooted(path))
        //    {
        //        this.mDataPath = path;
        //    }
        //    else
        //    {
        //        if (mDatabaseName != path)
        //        {
        //            mDatabaseName = path;
        //            this.mDataPath = System.IO.Path.Combine(mDataPath, path);
        //        }
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        public void CheckDataPathExist()
        {
            if(!System.IO.Directory.Exists(mDataPath))
            {
                System.IO.Directory.CreateDirectory(mDataPath);
            }
        }

        /// <summary>
        /// 获取数据路径
        /// </summary>
        /// <param name="databaseName">数据库名称</param>
        /// <param name="path">子路径</param>
        /// <returns></returns>
        public string GetDataPath(string databaseName,string path)
        {
            return System.IO.Path.Combine(mDataPath,databaseName, path);
        }

        /// <summary>
        /// 获取报警文件的路径
        /// </summary>
        /// <param name="databaseName">数据库名称</param>
        /// <returns></returns>
        public string GetAlarmDataPath(string databaseName)
        {
            return System.IO.Path.Combine(mDataPath, databaseName,"Alm");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public string GetDatabasePath(string database)
        {
            return System.IO.Path.Combine(mDataPath, database);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetApplicationFilePath(params string[] file)
        {
            string spath = mAppPath;
            foreach(var vv in file)
            {
                spath = System.IO.Path.Combine(spath, vv);
            }
            return spath;
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
