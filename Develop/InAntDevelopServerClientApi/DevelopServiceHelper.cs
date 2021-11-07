//==============================================================
//  Copyright (C) 2020  Inc. All rights reserved.
//
//==============================================================
//  Create by 种道洋 at 2020/3/28 22:49:54.
//  Version 1.0
//  种道洋
//==============================================================

using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Data.Common;
using Cdy.Ant;
using System.Xml.Linq;

namespace DBDevelopClientApi
{
    /// <summary>
    /// 
    /// </summary>
    public class DevelopServiceHelper
    {

        #region ... Variables  ...
        
        public static DevelopServiceHelper Helper = new DevelopServiceHelper();

        private AntDevelopServer.DevelopServer.DevelopServerClient mCurrentClient;

        private string mLoginId = string.Empty;

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...

        /// <summary>
        /// 
        /// </summary>
        public bool UseTls
        {
            get;
            set;
        } = true;


        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        private AntDevelopServer.DevelopServer.DevelopServerClient GetServicClient(string ip)
        {
            try
            {
               

                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                
                var httpClient = new HttpClient(httpClientHandler);

                Grpc.Net.Client.GrpcChannel grpcChannel;
                if (UseTls)
                {
                    grpcChannel = Grpc.Net.Client.GrpcChannel.ForAddress(@"https://" + ip + ":15001", new GrpcChannelOptions { HttpClient = httpClient });
                }
                else
                {

                    //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    grpcChannel = Grpc.Net.Client.GrpcChannel.ForAddress(@"http://" + ip + ":15001", new GrpcChannelOptions { HttpClient = httpClient });
                    //grpcChannel = Grpc.Net.Client.GrpcChannel.ForAddress(@"http://" + ip + ":5001");
                }
                return new AntDevelopServer.DevelopServer.DevelopServerClient(grpcChannel);
            }
            catch(Exception ex)
            {
                LoggerService.Service.Erro("DevelopService", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool Save(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.Save(new AntDevelopServer.GetRequest() { Database = database, LoginId = mLoginId}).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool CheckOpenDatabase(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.CheckOpenAntDatabase(new AntDevelopServer.DatabasesRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool StartDatabase(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.Start(new AntDevelopServer.DatabasesRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool StopDatabase(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.Stop(new AntDevelopServer.DatabasesRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool ReRunDatabase(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.ReRun(new AntDevelopServer.DatabasesRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool CancelToSaveDatabase(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.Cancel(new AntDevelopServer.GetRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool IsDatabaseRunning(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.IsAntDatabaseRunning(new AntDevelopServer.DatabasesRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool IsDatabaseDirty(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.IsAntDatabaseDirty(new AntDevelopServer.DatabasesRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        /// <summary>
        /// 放弃更改
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool Cancel(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.Cancel(new AntDevelopServer.GetRequest() { Database = database, LoginId = mLoginId }).Result;
            }
            return false;
        }

        public bool IsAdmin()
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var lres = mCurrentClient.IsAdmin(new AntDevelopServer.GetRequest() { LoginId = mLoginId });            //var sid = await client.LoginAsync(new DBDevelopService.LoginRequest() { UserName = "admin", Password = "12345", Database = "local" });
                    if (lres != null)
                    {
                        return lres.Result;
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Service.Erro("DevelopService", ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public string Login(string ip,string user,string pass)
        {
            mCurrentClient = GetServicClient(ip);
            if (mCurrentClient != null)
            {
                try
                {
                    var lres = mCurrentClient.Login(new AntDevelopServer.LoginRequest() { UserName = user, Password = pass });            //var sid = await client.LoginAsync(new DBDevelopService.LoginRequest() { UserName = "admin", Password = "12345", Database = "local" });
                    if (lres != null)
                    {
                        mLoginId = lres.LoginId;
                        return lres.LoginId;
                    }
                }
                catch(Exception ex)
                {
                    LoggerService.Service.Erro("DevelopService", ex.Message);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Logout()
        {
            if (mCurrentClient != null)
            {
                try
                {
                    var lres = mCurrentClient.Logout(new AntDevelopServer.LogoutRequest() { LoginId=mLoginId });
                    if (lres != null)
                    {
                        return lres.Result;
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Service.Erro("DevelopService", ex.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,string> ListDatabase()
        {
            var re = new Dictionary<string, string>();
            try
            {
                if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
                {
                    var vv = mCurrentClient.QueryAntDatabase(new AntDevelopServer.QueryDatabaseRequest() { LoginId = mLoginId }).Database.ToList();
                    foreach(var vvv in vv)
                    {
                        re.Add(vvv.Key, vvv.Value);
                    }
                }
            }
            catch
            {

            }
            return re;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public Dictionary<string, string> ListMarsDatabase()
        //{
        //    var re = new Dictionary<string, string>();
        //    try
        //    {
        //        if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
        //        {
        //            var vv = mCurrentClient.QueryMarsDatabase(new AntDevelopServer.QueryDatabaseRequest() { LoginId = mLoginId }).Database.ToList();
        //            foreach (var vvv in vv)
        //            {
        //                re.Add(vvv.Key, vvv.Value);
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }
        //    return re;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="desc"></param>
        ///// <returns></returns>
        //public bool NewDatabase(string name,string desc)
        //{
        //    if(mCurrentClient!=null&&!string.IsNullOrEmpty(mLoginId))
        //    {
        //       return mCurrentClient.NewAntDatabase(new AntDevelopServer.NewDatabaseRequest() { Database = name, LoginId = mLoginId,Desc=string.IsNullOrEmpty(desc)?"":desc }).Result;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Tuple<string,string,string>> QueryTagGroups(string database)
        {
            List<Tuple<string, string, string>> re = new List<Tuple<string, string, string>>();
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                foreach(var vv in mCurrentClient.GetTagGroup(new AntDevelopServer.GetRequest() { Database = database, LoginId = mLoginId }).Group)
                {
                    re.Add(new Tuple<string, string, string>(vv.Name,vv.Description, vv.Parent));
                }
            }
            return re;
        }

        public void QueryAllTag(string database,Action<int,int,Dictionary<int, Tagbase>> callback)
        {
           
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                int idx = 0;
                int count = 0;
                do
                {
                    Dictionary<int, Tagbase> re = new Dictionary<int, Tagbase>();
                    var result = mCurrentClient.GetAllTag(new AntDevelopServer.GetTagByGroupRequest() { Database = database, LoginId = mLoginId, Index = idx });

                    idx = result.Index;
                    count = result.Count;

                    if (!result.Result) break;

                    Dictionary<int, Tagbase> mRealTag = new Dictionary<int, Tagbase>();
                    foreach (var vv in result.Tags)
                    {
                        var tag = GetTag((int)vv.TagType);
                        tag.Id = (int)vv.Id;
                        if (tag is AlarmTag)
                            (tag as AlarmTag).LinkTag = vv.LinkTag;
                        tag.Name = vv.Name;
                        tag.Desc = vv.Desc;
                        tag.Group = vv.Group;
                        tag.IsEnable = vv.IsEnable;
                        tag.CustomContent1 = vv.CustomContent1;
                        tag.CustomContent2 = vv.CustomContent2;
                        tag.CustomContent3 = vv.CustomContent3;
                        if(tag is SimpleAlarmTag)
                        {
                            (tag as SimpleAlarmTag).AlarmLevel = (AlarmLevel)vv.AlarmLevel;
                        }
                        DeseriseAlarmContent(tag, vv.AlarmContent);
                        mRealTag.Add(tag.Id, tag);
                    }


                    re = mRealTag;
                    callback(idx, count, re);
                    idx++;
                }
                while (idx < count);
            }
       
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public Dictionary<int, Tagbase> QueryAllTag(string database)
        {
            Dictionary<int, Tagbase> re = new Dictionary<int, Tagbase>();
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                int idx = 0;
                int count = 0;
                do
                {
                    var result = mCurrentClient.GetAllTag(new AntDevelopServer.GetTagByGroupRequest() { Database = database, LoginId = mLoginId,Index=idx });

                    idx = result.Index;
                    count = result.Count;

                    if (!result.Result) break;

                    Dictionary<int, Tagbase> mRealTag = new Dictionary<int, Tagbase>();
                    foreach (var vv in result.Tags)
                    {
                        var tag = GetTag((int)vv.TagType);
                        tag.Id = (int)vv.Id;
                        if (tag is AlarmTag)
                            (tag as AlarmTag).LinkTag = vv.LinkTag;
                        tag.Name = vv.Name;
                        tag.Desc = vv.Desc;
                        tag.Group = vv.Group;
                        tag.IsEnable = vv.IsEnable;
                        tag.CustomContent1 = vv.CustomContent1;
                        tag.CustomContent2 = vv.CustomContent2;
                        tag.CustomContent3 = vv.CustomContent3;
                        if (tag is SimpleAlarmTag)
                        {
                            (tag as SimpleAlarmTag).AlarmLevel = (AlarmLevel)vv.AlarmLevel;
                        }
                        DeseriseAlarmContent(tag, vv.AlarmContent);
                        mRealTag.Add(tag.Id, tag);
                    }

                    re = mRealTag;

                    idx++;
                }
                while (idx < count);
            }
            return re;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="group"></param>
        /// <param name="callback"></param>
        public void QueryTagByGroup(string database, string group, Action<int, int, Dictionary<int, Tagbase>> callback, Dictionary<string, string> mFilters = null)
        {
            int idx = 0;
            int totalcount=0;
            int tagcount;
            do
            {
                var re = QueryTagByGroup(database, group, idx, out totalcount, out tagcount,mFilters);
                callback(idx, totalcount, re);
                idx++;
            }
            while (idx < totalcount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="group"></param>
        /// <param name="totalCount"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Dictionary<int, Tagbase> QueryTagByGroup(string database, string group, int index, out int totalCount,out int tagCount,Dictionary<string,string> mFilters=null)
        {
            Dictionary<int, Tagbase> re = new Dictionary<int, Tagbase>();
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                int idx = index;
                var req = new AntDevelopServer.GetTagByGroupRequest() { Database = database, LoginId = mLoginId, Group = group, Index = idx };
                if (mFilters != null)
                {
                    foreach (var vv in mFilters)
                    {
                        req.Filters.Add(new AntDevelopServer.FilterMessageItem() { Key = vv.Key, Value = vv.Value });
                    }
                }

                var result = mCurrentClient.GetTagByGroup(req);
                tagCount = result.TagCount;

                totalCount = result.Count;

                if (result.Result)
                {
                    Dictionary<int, Tagbase> mRealTag = new Dictionary<int, Tagbase>();
                    foreach (var vv in result.Tags)
                    {
                        var tag = GetTag((int)vv.TagType);
                        tag.Id = (int)vv.Id;
                        if (tag is AlarmTag)
                            (tag as AlarmTag).LinkTag = vv.LinkTag;
                        tag.Name = vv.Name;
                        tag.Desc = vv.Desc;
                        tag.Group = vv.Group;
                        tag.IsEnable = vv.IsEnable;
                        tag.CustomContent1 = vv.CustomContent1;
                        tag.CustomContent2 = vv.CustomContent2;
                        tag.CustomContent3 = vv.CustomContent3;
                        if (tag is SimpleAlarmTag)
                        {
                            (tag as SimpleAlarmTag).AlarmLevel = (AlarmLevel)vv.AlarmLevel;
                        }
                        DeseriseAlarmContent(tag, vv.AlarmContent);
                        mRealTag.Add(tag.Id, tag);
                    }

                    re = mRealTag;
                }
            }
            else
            {
                totalCount = -1;
                tagCount = 0;
            }
          
            return re;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public Dictionary<int, Tagbase> QueryTagByGroup(string database,string group, Dictionary<string, string> mFilters = null)
        {
            Dictionary<int, Tagbase> re = new Dictionary<int, Tagbase>();
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                int idx = 0;
                int count = 0;
                do
                {
                    var req = new AntDevelopServer.GetTagByGroupRequest() { Database = database, LoginId = mLoginId, Group = group, Index = idx };
                    if (mFilters != null)
                    {
                        foreach (var vv in mFilters)
                        {
                            req.Filters.Add(new AntDevelopServer.FilterMessageItem() { Key = vv.Key, Value = vv.Value });
                        }
                    }

                    var result = mCurrentClient.GetTagByGroup(req);

                    idx = result.Index;
                    count = result.Count;

                    if (!result.Result) break;

                    Dictionary<int, Tagbase> mRealTag = new Dictionary<int, Tagbase>();
                    foreach (var vv in result.Tags)
                    {
                        var tag = GetTag((int)vv.TagType);
                        tag.Id = (int)vv.Id;
                        if (tag is AlarmTag)
                            (tag as AlarmTag).LinkTag = vv.LinkTag;
                        tag.Name = vv.Name;
                        tag.Desc = vv.Desc;
                        tag.Group = vv.Group;
                        tag.IsEnable = vv.IsEnable;
                        tag.CustomContent1 = vv.CustomContent1;
                        tag.CustomContent2 = vv.CustomContent2;
                        tag.CustomContent3 = vv.CustomContent3;
                        if (tag is SimpleAlarmTag)
                        {
                            (tag as SimpleAlarmTag).AlarmLevel = (AlarmLevel)vv.AlarmLevel;
                        }
                        DeseriseAlarmContent(tag, vv.AlarmContent);
                        mRealTag.Add(tag.Id, tag);
                    }



                    re = mRealTag;

                    idx++;
                }
                while (idx < count);
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool UpdateTag(string database, Tagbase tag)
        {
            bool re = true;
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                re &= mCurrentClient.UpdateTag(new AntDevelopServer.UpdateTagRequestMessage() { Database = database, LoginId = mLoginId, Tag = ConvertToRealTagMessage(tag) }).Result;
            }
            else
            {
                re = false;
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool AddTag(string database, Tagbase tag,out int id)
        {
            bool re = false;
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                var res = mCurrentClient.AddTag(new AntDevelopServer.AddTagRequestMessage() { Database = database, LoginId = mLoginId, RealTag = ConvertToRealTagMessage(tag) });
                re = res.Result;
                id = res.TagId;
            }
            else
            {
                id = -1;
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tag"></param>
        /// <param name="mode"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Import(string database, Tagbase tag, int mode,out int id)
        {
            bool re = false;
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                var res = mCurrentClient.Import(new AntDevelopServer.ImportTagRequestMessage() { Database = database, LoginId = mLoginId, RealTag = ConvertToRealTagMessage(tag),Mode=mode });
                re = res.Result;
                id = res.TagId;
            }
            else
            {
                id = -1;
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool ClearTagByGroup(string database, string group)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                var res = mCurrentClient.ClearTag(new AntDevelopServer.ClearTagRequestMessage() { Database = database, LoginId = mLoginId, GroupFullName = group });
                return res.Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool ClearTagAll(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                var res = mCurrentClient.ClearAllTag(new AntDevelopServer.GetRequest() { Database = database, LoginId = mLoginId });
                return res.Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="name"></param>
        /// <param name="parentName"></param>
        public string AddTagGroup(string database,string name,string parentName)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.AddTagGroup(new AntDevelopServer.AddGroupRequest() { Database = database, LoginId = mLoginId, Name = name, ParentName = parentName }).Group;
            }
            return string.Empty;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="groupname"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public bool UpdateGroupDescription(string database, string groupname, string desc)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.UpdateGroupDescription(new AntDevelopServer.UpdateGroupDescriptionRequest() { Database = database, LoginId = mLoginId, GroupName = groupname, Desc = desc }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sourceGroup"></param>
        /// <param name="targetParentGroup"></param>
        /// <returns></returns>
        public string PasteTagGroup(string database, string sourceGroup,string targetParentGroup)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.PasteTagGroup(new AntDevelopServer.PasteGroupRequest() { Database = database, LoginId = mLoginId, GroupFullName = sourceGroup, TargetParentName = targetParentGroup }).Group;
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool ReNameTagGroup(string database,string oldName,string newName)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.RenameTagGroup(new AntDevelopServer.RenameGroupRequest() { Database = database, LoginId = mLoginId,OldFullName=oldName,NewName=newName }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveTagGroup(string database,string name)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                return mCurrentClient.RemoveTagGroup(new AntDevelopServer.RemoveGroupRequest() { Database = database, LoginId = mLoginId, Name = name }).Result;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Remove(string database, int id)
        {
            var msg = new AntDevelopServer.RemoveTagMessageRequest() { Database = database, LoginId = mLoginId };
            msg.TagId.Add(id);
            return mCurrentClient.RemoveTag(msg).Result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private AntDevelopServer.TagMessage ConvertToRealTagMessage(Tagbase tag)
        {
            AntDevelopServer.TagMessage re = new AntDevelopServer.TagMessage();
            re.Id = tag.Id;
            if (tag is AlarmTag)
                re.LinkTag = (tag as AlarmTag).LinkTag;
            else
                re.LinkTag = string.Empty;

            re.Name = tag.Name;
            re.TagType = (uint)tag.Type;
            re.Group = tag.Group;
            re.Desc = tag.Desc;
            re.IsEnable = tag.IsEnable;

            re.CustomContent1 = tag.CustomContent1;
            re.CustomContent2 = tag.CustomContent2;
            re.CustomContent3 = tag.CustomContent3;
            if (tag is AlarmTag)
            {
                re.LinkTag = (tag as Cdy.Ant.AlarmTag).LinkTag;
            }
            if (tag is Cdy.Ant.SimpleAlarmTag)
            {
                re.AlarmLevel = (int)(tag as SimpleAlarmTag).AlarmLevel;

            }
            re.AlarmContent = SeriseAlarmContent(tag);

            return re;
        }

       



        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public Setting GetServerSetting(string database)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                var res = mCurrentClient.GetServerSetting(new AntDevelopServer.DatabasesRequest() { Database = database, LoginId = mLoginId });
                return new Setting() { ApiType = res.Value.ApiKey, ApiData = !string.IsNullOrEmpty(res.Value.ApiValue) ? XElement.Parse(res.Value.ApiValue):null,ProxyType = res.Value.ProxyKey, ProxyData = !string.IsNullOrEmpty(res.Value.ProxyValue) ? XElement.Parse(res.Value.ProxyValue) : null };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool SetServerSetting(string database, Setting port)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                var setting = new AntDevelopServer.SetSettingRequest() { Database = database, LoginId = mLoginId,Value = new AntDevelopServer.SettingMessage()};
                setting.Value.ApiKey = port.ApiType;
                setting.Value.ApiValue = port.ApiData != null ? port.ApiData.ToString() : "";
                setting.Value.ProxyKey = port.ProxyType;
                setting.Value.ProxyValue = port.ProxyData != null ? port.ProxyData.ToString() : "";
                var res = mCurrentClient.SetServerSetting(setting);
                return res.Result;
            }
            else
            {
                return false;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagType"></param>
        /// <returns></returns>
        private Tagbase GetTag(int tagType)
        {
            switch (tagType)
            {
                case (int)TagType.AnalogAlarm:
                    return new AnalogAlarmTag();
                case (int)TagType.AnalogRangeAlarm:
                    return new AnalogRangeAlarmTag();
                case (int)TagType.DelayDigitalAlarm:
                    return new DelayDigitalAlarmTag();
                case (int)TagType.DigitalAlarm:
                    return new DigitalAlarmTag();
                case (int)TagType.OneRange:
                    return new OneRangeAlarmTag();
                case (int)TagType.Pulse:
                    return new PulseAlarmTag();
                case (int)TagType.Script:
                    return new ScriptTag();
                case (int)TagType.StringAlarm:
                    return new StringAlarmTag();
                case (int)TagType.ThreeRange:
                    return new ThreeRangeAlarmTag();
                case (int)TagType.TwoRange:
                    return new TwoRangeAlarmTag();
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagids"></param>
        /// <returns></returns>
        public Dictionary<int,int> ResetTagIds(string database,List<int> tagids,int startId)
        {
            Dictionary<int, int> re = new Dictionary<int, int>();
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                var qq = new AntDevelopServer.ResetTagIdRequest() { Database = database, LoginId = mLoginId, StartId = startId };
                qq.TagIds.AddRange(tagids);
                var res = mCurrentClient.ResetTagId(qq);
                var sval = res.TagIds;
                if (sval.Count>0)
                {
                    foreach(var vv in sval)
                    {
                        re.Add(vv.Key, vv.Value);
                    }
                }
            }
            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private string SeriseAlarmContent(Cdy.Ant.Tagbase tag)
        {
            StringBuilder re = new StringBuilder();
            if (tag is Cdy.Ant.AnalogAlarmTag)
            {
                var vtag = (tag as Cdy.Ant.AnalogAlarmTag);
                if (vtag.HighHighValue != null)
                {
                    re.Append("HH:" + vtag.HighHighValue.ToString() + ";");
                }
                if (vtag.HighValue != null)
                {
                    re.Append("H:" + vtag.HighValue.ToString() + ";");
                }
                if (vtag.LowValue != null)
                {
                    re.Append("L:" + vtag.LowValue.ToString() + ";");
                }
                if (vtag.LowLowValue != null)
                {
                    re.Append("LL:" + vtag.LowLowValue.ToString() + ";");
                }
            }
            else if (tag is Cdy.Ant.AnalogRangeAlarmTag)
            {
                foreach (var vv in (tag as Cdy.Ant.AnalogRangeAlarmTag).Items)
                {
                    re.Append(vv.ToString() + ";");
                }
            }
            else if (tag is Cdy.Ant.DigitalAlarmTag)
            {
                var vtag = tag as Cdy.Ant.DigitalAlarmTag;
                //re.Append("Delay:" + vtag.Delay+";");
                re.Append("Value:" + vtag.Value + ";");
            }
            else if (tag is Cdy.Ant.DelayDigitalAlarmTag)
            {
                var vtag = tag as Cdy.Ant.DelayDigitalAlarmTag;
                re.Append("Delay:" + vtag.Delay + ";");
                re.Append("Value:" + vtag.Value + ";");
            }
            else if (tag is Cdy.Ant.PulseAlarmTag)
            {
                var vtag = tag as Cdy.Ant.PulseAlarmTag;
                re.Append("PulseType:" + (byte)vtag.PulseType + ";");
            }
            else if (tag is Cdy.Ant.StringAlarmTag)
            {
                var vtag = tag as Cdy.Ant.StringAlarmTag;
                re.Append(vtag.Value + ";");
            }
            else if (tag is Cdy.Ant.ScriptTag)
            {
                var vtag = tag as Cdy.Ant.ScriptTag;
                re.Append(vtag.Expresse + ";");
            }
            else if (tag is Cdy.Ant.OneRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.OneRangeAlarmTag;
                re.Append("IsInRangeAlarm:" + vtag.IsInRangeAlarm + ";");
                re.Append("MinValue:" + vtag.MinValue + ";");
                re.Append("MaxValue:" + vtag.MaxValue + ";");
            }
            else if (tag is Cdy.Ant.TwoRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.TwoRangeAlarmTag;
                re.Append("IsInRangeAlarm:" + vtag.IsInRangeAlarm + ";");
                StringBuilder ss = new StringBuilder();
                foreach (var vv in vtag.AlarmDatas)
                {
                    ss.Append(vv.X + "," + vv.Y + "|");
                }
                ss.Length = ss.Length > 0 ? ss.Length - 1 : ss.Length;
                re.Append("AlarmDatas:" + ss.ToString() + ";");
            }
            else if (tag is Cdy.Ant.ThreeRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.ThreeRangeAlarmTag;
                re.Append("IsInRangeAlarm:" + vtag.IsInRangeAlarm + ";");
                StringBuilder ss = new StringBuilder();
                foreach (var vv in vtag.AlarmDatas)
                {
                    ss.Append(vv.X + "," + vv.Y + "," + vv.Z + "|");
                }
                ss.Length = ss.Length > 0 ? ss.Length - 1 : ss.Length;
                re.Append("AlarmDatas:" + ss.ToString() + ";");
            }
            re.Length = re.Length > 0 ? re.Length - 1 : re.Length;
            return re.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="content"></param>
        private void DeseriseAlarmContent(Cdy.Ant.Tagbase tag, string content)
        {
            if (tag is Cdy.Ant.AnalogAlarmTag)
            {
                var vtag = (tag as Cdy.Ant.AnalogAlarmTag);
                string[] ss = content.Split(new char[] { ';' });
                if (ss.Length > 0)
                {
                    foreach (var vv in ss)
                    {
                        string[] vss = vv.Split(new char[] { ':' });
                        if (vss[0] == "HH")
                        {
                            vtag.HighHighValue = new Cdy.Ant.AnalogAlarmItem().LoadFromString(vss[1]);
                        }
                        else if (vss[0] == "H")
                        {
                            vtag.HighValue = new Cdy.Ant.AnalogAlarmItem().LoadFromString(vss[1]);
                        }
                        else if (vss[0] == "L")
                        {
                            vtag.LowValue = new Cdy.Ant.AnalogAlarmItem().LoadFromString(vss[1]);
                        }
                        else if (vss[0] == "LL")
                        {
                            vtag.LowLowValue = new Cdy.Ant.AnalogAlarmItem().LoadFromString(vss[1]);
                        }
                    }
                }

            }
            else if (tag is Cdy.Ant.AnalogRangeAlarmTag)
            {
                var vtag = (tag as Cdy.Ant.AnalogRangeAlarmTag);
                vtag.Items = new List<Cdy.Ant.AnalogRangeAlarmItem>();
                string[] ss = content.Split(new char[] { ';' });
                foreach (var vvs in ss)
                {
                    vtag.Items.Add(new Cdy.Ant.AnalogRangeAlarmItem().LoadFromString(vvs));
                }
            }
            else if (tag is Cdy.Ant.DigitalAlarmTag)
            {
                var vtag = tag as Cdy.Ant.DigitalAlarmTag;
                string[] ss = content.Split(new char[] { ':' });
                vtag.Value = bool.Parse(ss[1]);
            }
            else if (tag is Cdy.Ant.DelayDigitalAlarmTag)
            {
                var vtag = tag as Cdy.Ant.DelayDigitalAlarmTag;
                string[] ss = content.Split(new char[] { ';' });
                if (ss.Length > 0)
                {
                    foreach (var vv in ss)
                    {
                        string[] vss = vv.Split(new char[] { ':' });
                        if (vss[0] == "Delay")
                        {
                            vtag.Delay = double.Parse(vss[1]);
                        }
                        else if (vss[0] == "Value")
                        {
                            vtag.Value = bool.Parse(vss[1]);
                        }
                    }
                }
            }
            else if (tag is Cdy.Ant.PulseAlarmTag)
            {
                var vtag = tag as Cdy.Ant.PulseAlarmTag;
                string[] ss = content.Split(new char[] { ':' });
                vtag.PulseType = (Cdy.Ant.PulseAlarmType)(int.Parse(ss[1]));
            }
            else if (tag is Cdy.Ant.StringAlarmTag)
            {
                var vtag = tag as Cdy.Ant.StringAlarmTag;
                vtag.Value = content;
            }
            else if (tag is Cdy.Ant.ScriptTag)
            {
                var vtag = tag as Cdy.Ant.ScriptTag;
                vtag.Expresse = content;
            }
            else if (tag is Cdy.Ant.OneRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.OneRangeAlarmTag;
                string[] ss = content.Split(new char[] { ';' });
                if (ss.Length > 0)
                {
                    foreach (var vv in ss)
                    {
                        string[] vss = vv.Split(new char[] { ':' });
                        if (vss[0] == "IsInRangeAlarm")
                        {
                            vtag.IsInRangeAlarm = bool.Parse(vss[1]);
                        }
                        else if (vss[0] == "MinValue")
                        {
                            vtag.MinValue = double.Parse(vss[1]);
                        }
                        else if (vss[0] == "MaxValue")
                        {
                            vtag.MaxValue = double.Parse(vss[1]);
                        }
                    }
                }
            }
            else if (tag is Cdy.Ant.TwoRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.TwoRangeAlarmTag;
                string[] ss = content.Split(new char[] { ';' });
                if (ss.Length > 0)
                {
                    foreach (var vv in ss)
                    {
                        string[] vss = vv.Split(new char[] { ':' });
                        if (vss[0] == "IsInRangeAlarm")
                        {
                            vtag.IsInRangeAlarm = bool.Parse(vss[1]);
                        }
                        else if (vss[0] == "AlarmDatas")
                        {
                            vtag.AlarmDatas = new List<Cdy.Ant.Point>();
                            var pps = vss[1].Split(new char[] { '|' });
                            foreach (var vvp in pps)
                            {
                                var vpps = vvp.Split(new char[] { ',' });
                                Cdy.Ant.Point ppt = new Cdy.Ant.Point() { X = double.Parse(vpps[0]), Y = double.Parse(vpps[1]) };
                                vtag.AlarmDatas.Add(ppt);
                            }
                        }
                    }
                }
            }
            else if (tag is Cdy.Ant.ThreeRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.ThreeRangeAlarmTag;
                string[] ss = content.Split(new char[] { ';' });
                if (ss.Length > 0)
                {
                    foreach (var vv in ss)
                    {
                        string[] vss = vv.Split(new char[] { ':' });
                        if (vss[0] == "IsInRangeAlarm")
                        {
                            vtag.IsInRangeAlarm = bool.Parse(vss[1]);
                        }
                        else if (vss[0] == "AlarmDatas")
                        {
                            vtag.AlarmDatas = new List<Cdy.Ant.Point3D>();
                            var pps = vss[1].Split(new char[] { '|' });
                            foreach (var vvp in pps)
                            {
                                var vpps = vvp.Split(new char[] { ',' });
                                Cdy.Ant.Point3D ppt = new Cdy.Ant.Point3D() { X = double.Parse(vpps[0]), Y = double.Parse(vpps[1]), Z = double.Parse(vpps[2]) };
                                vtag.AlarmDatas.Add(ppt);
                            }
                        }
                    }
                }
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }
}
