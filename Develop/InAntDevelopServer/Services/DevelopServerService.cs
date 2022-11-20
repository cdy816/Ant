using AntDevelopServer;
using Cdy.Ant;
using DBDevelopService;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace InAntDevelopServer
{
    /// <summary>
    /// 
    /// </summary>
    public class DevelopServerService : AntDevelopServer.DevelopServer.DevelopServerBase
    {
        public const int PageCount = 500;

        private readonly ILogger<DevelopServerService> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public DevelopServerService(ILogger<DevelopServerService> logger)
        {
            _logger = logger;
        }

        #region Login
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            LoginReply re = new LoginReply() { LoginId = SecurityManager.Manager.Login(request.UserName, request.Password) };
            return Task.FromResult(re);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> Logout(LogoutRequest request, ServerCallContext context)
        {
            SecurityManager.Manager.Logout(request.LoginId);
            return Task.FromResult(new BoolResultReplay() { Result = true });
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CheckLoginId(string id, string database = "")
        {
            return SecurityManager.Manager.CheckKeyAvaiable(id) && (SecurityManager.Manager.CheckDatabase(id, database) || SecurityManager.Manager.IsAdmin(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> IsAdmin(GetRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BoolResultReplay() { Result = SecurityManager.Manager.IsAdmin(request.LoginId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetSettingReplay> GetServerSetting(DatabasesRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new GetSettingReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                return Task.FromResult(new GetSettingReplay() { Result = true, Value = new SettingMessage() { ApiKey = db.Setting.ApiType, ApiValue = db.Setting.ApiData!=null? db.Setting.ApiData.ToString():"", ProxyKey = db.Setting.ProxyType, ProxyValue = db.Setting.ProxyData!=null? db.Setting.ProxyData.ToString():"" } });
            }
            return Task.FromResult(new GetSettingReplay() { Result = false });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> SetServerSetting(SetSettingRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                db.Setting = new Setting() { ApiType = request.Value.ApiKey, ProxyType = request.Value.ProxyKey };
                if(!string.IsNullOrEmpty(request.Value.ApiValue))
                {
                    db.Setting.ApiData = XElement.Parse(request.Value.ApiValue);
                }

                if (!string.IsNullOrEmpty(request.Value.ProxyValue))
                {
                    db.Setting.ProxyData = XElement.Parse(request.Value.ProxyValue);
                }

                return Task.FromResult(new BoolResultReplay() { Result = true });
            }
            return Task.FromResult(new BoolResultReplay() { Result = false });
        }

        

        #endregion

        #region database

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool IsAdmin(string id)
        {
            return SecurityManager.Manager.CheckKeyAvaiable(id) && SecurityManager.Manager.IsAdmin(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool HasNewDatabasePermission(string id)
        {
            return SecurityManager.Manager.CheckKeyAvaiable(id) && SecurityManager.Manager.HasNewDatabasePermission(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool HasDeleteDatabasePerssion(string id)
        {
            return SecurityManager.Manager.CheckKeyAvaiable(id) && SecurityManager.Manager.HasDeleteDatabasePermission(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> CheckOpenAntDatabase(DatabasesRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                DbManager.Instance.CheckAndContinueLoadDatabase(db);
            }
            else
            {
                DbManager.Instance.NewDB(request.Database, request.Database);
                var user = SecurityManager.Manager.GetUser(request.LoginId);
                db = DbManager.Instance.GetDatabase(request.Database);
                db.Setting.ApiType = ApiFactory.Factory.ListDevelopApis().First();
                db.Setting.ApiData = ApiFactory.Factory.GetDevelopInstance(db.Setting.ApiType).Save();

                db.Setting.ProxyType = ProxyServiceFactory.Factory.ProxyService.First();
                db.Setting.ProxyData = ProxyServiceFactory.Factory.GetDevelopInstance(db.Setting.ProxyType).Save();

                user.Databases.Add(request.Database);
            }
            return Task.FromResult(new BoolResultReplay() { Result = true });
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> NewAntDatabase(NewDatabaseRequest request, ServerCallContext context)
        {
            if (!IsAdmin(request.LoginId) && !HasNewDatabasePermission(request.LoginId))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }

            lock (DbManager.Instance)
            {
                var db = DbManager.Instance.GetDatabase(request.Database);
                if (db != null)
                {
                    return Task.FromResult(new BoolResultReplay() { Result = false, ErroMessage = "数据库已经存在!" });
                }
                else
                {
                    DbManager.Instance.NewDB(request.Database, request.Desc);
                    var user = SecurityManager.Manager.GetUser(request.LoginId);

                    user.Databases.Add(request.Database);
                }
            }
            return Task.FromResult(new BoolResultReplay() { Result = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<QueryDatabaseReplay> QueryAntDatabase(QueryDatabaseRequest request, ServerCallContext context)
        {
            if (!SecurityManager.Manager.CheckKeyAvaiable(request.LoginId))
            {
                return Task.FromResult(new QueryDatabaseReplay() { Result = false });
            }

            QueryDatabaseReplay re = new QueryDatabaseReplay() { Result = true };
            var user = SecurityManager.Manager.GetUser(request.LoginId);
            var dbs = user.Databases;
            foreach (var vv in DbManager.Instance.ListDatabase())
            {
                if (dbs.Contains(vv) || user.IsAdmin)
                {
                    string desc = "";
                    re.Database.Add(new KeyValueMessage() { Key = vv, Value = desc });
                }
            }
            foreach (var vv in DbManager.Instance.ListMarsDatabase())
            {
                if (dbs.Contains(vv) || user.IsAdmin)
                {
                    string desc = "";
                    re.Database.Add(new KeyValueMessage() { Key = vv, Value = desc });
                }
            }
            return Task.FromResult(re);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public override Task<QueryDatabaseReplay> QueryMarsDatabase(QueryDatabaseRequest request, ServerCallContext context)
        //{
        //    if (!SecurityManager.Manager.CheckKeyAvaiable(request.LoginId))
        //    {
        //        return Task.FromResult(new QueryDatabaseReplay() { Result = false });
        //    }

        //    QueryDatabaseReplay re = new QueryDatabaseReplay() { Result = true };
        //    var user = SecurityManager.Manager.GetUser(request.LoginId);
        //    var dbs = user.Databases;
        //    foreach (var vv in DbManager.Instance.ListMarsDatabase())
        //    {
        //        if (dbs.Contains(vv) || user.IsAdmin)
        //        {
        //            string desc = "";
        //            re.Database.Add(new KeyValueMessage() { Key = vv, Value = desc });
        //        }
        //    }
        //    return Task.FromResult(re);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> Save(GetRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    Cdy.Ant.AlarmDatabaseSerise serise = new Cdy.Ant.AlarmDatabaseSerise() { Database = db };
                    serise.Save();
                }
            }
            return Task.FromResult(new BoolResultReplay() { Result = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> Cancel(GetRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            lock (DbManager.Instance)
            {
                DbManager.Instance.Reload(request.Database);
            }
            return Task.FromResult(new BoolResultReplay() { Result = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> IsAntDatabaseDirty(DatabasesRequest request, ServerCallContext context)
        {
            if (!SecurityManager.Manager.CheckKeyAvaiable(request.LoginId))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                DbManager.Instance.CheckAndContinueLoadDatabase(db);
                return Task.FromResult(new BoolResultReplay() { Result = db.IsDirty });
            }
            else
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> IsAntDatabaseRunning(DatabasesRequest request, ServerCallContext context)
        {
            if (!SecurityManager.Manager.CheckKeyAvaiable(request.LoginId))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            return Task.FromResult(new BoolResultReplay() { Result = ServiceLocator.Locator.Resolve<IDatabaseManager>().IsRunning(request.Database) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> Start(DatabasesRequest request, ServerCallContext context)
        {
            if (!IsAdmin(request.LoginId))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            return Task.FromResult(new BoolResultReplay() { Result = ServiceLocator.Locator.Resolve<IDatabaseManager>().Start(request.Database) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> Stop(DatabasesRequest request, ServerCallContext context)
        {
            if (!IsAdmin(request.LoginId))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            return Task.FromResult(new BoolResultReplay() { Result = ServiceLocator.Locator.Resolve<IDatabaseManager>().Stop(request.Database) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> ReRun(DatabasesRequest request, ServerCallContext context)
        {
            if (!IsAdmin(request.LoginId))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            return Task.FromResult(new BoolResultReplay() { Result = ServiceLocator.Locator.Resolve<IDatabaseManager>().Rerun(request.Database) });
        }

        #endregion

        #region Tag

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetTagMessageReply> QueryTag(QueryMessageRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new GetTagMessageReply() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                if (request.Conditions.Count == 0)
                    return Task.FromResult(new GetTagMessageReply() { Result = true });

                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    IEnumerable<Cdy.Ant.Tagbase> htags = db.Tags.Values;
                    foreach (var vv in request.Conditions)
                    {
                        switch (vv.Key.ToLower())
                        {
                            case "id":
                                htags = htags.Where(e => e.Id == int.Parse(vv.Value));
                                break;
                            case "name":
                                htags = htags.Where(e => e.Name.Contains(vv.Value));
                                break;
                            case "type":
                                htags = htags.Where(e => e.Type == (Cdy.Ant.TagType)int.Parse(vv.Value));
                                break;
                            case "group":
                                htags = htags.Where(e => e.Group.Contains(vv.Value));
                                break;
                            case "desc":
                                htags = htags.Where(e => e.Desc.Contains(vv.Value));
                                break;
                        }
                    }

                    List<TagMessage> re = new List<TagMessage>();
                    foreach (var vv in htags)
                    {
                        var vtag = new TagMessage() { Id = vv.Id, Name = vv.Name, Desc = vv.Desc, Group = vv.Group, TagType = (uint)vv.Type,IsEnable=vv.IsEnable,CustomContent1 = vv.CustomContent1,CustomContent2=vv.CustomContent2,CustomContent3=vv.CustomContent3 };
                        vtag.LinkTag = (vv is Cdy.Ant.AlarmTag) ? (vv as Cdy.Ant.AlarmTag).LinkTag : string.Empty;
                        vtag.AlarmLevel = (vv is Cdy.Ant.SimpleAlarmTag) ? (int)((vv as Cdy.Ant.SimpleAlarmTag).AlarmLevel): 0;
                        vtag.AlarmContent = SeriseAlarmContent(vv);
                        re.Add(vtag);
                    }

                    var msg = new GetTagMessageReply() { Result = true };
                    msg.Tags.AddRange(re);
                    return Task.FromResult(msg);
                }
            }
            return Task.FromResult(new GetTagMessageReply() { Result = true });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetTagMessageReply> GetAllTag(GetTagByGroupRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new GetTagMessageReply() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            List<TagMessage> re = new List<TagMessage>();
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    foreach (var vv in db.ListAllTags())
                    {
                        var vtag = new TagMessage() { Id = vv.Id, Name = vv.Name, Desc = vv.Desc, Group = vv.Group, TagType = (uint)vv.Type, IsEnable = vv.IsEnable, CustomContent1 = vv.CustomContent1, CustomContent2 = vv.CustomContent2, CustomContent3 = vv.CustomContent3 };
                        vtag.LinkTag = (vv is Cdy.Ant.AlarmTag) ? (vv as Cdy.Ant.AlarmTag).LinkTag : string.Empty;
                        vtag.AlarmLevel = (vv is Cdy.Ant.SimpleAlarmTag) ? (int)((vv as Cdy.Ant.SimpleAlarmTag).AlarmLevel) : 0;
                        vtag.AlarmContent = SeriseAlarmContent(vv);
                        re.Add(vtag);
                    }
                }
            }
            var msg = new GetTagMessageReply() { Result = true };
            msg.Tags.AddRange(re);
            return Task.FromResult(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="res"></param>
        /// <param name="vfilters"></param>
        /// <returns></returns>
        private IEnumerable<Tagbase> FilterTags(AlarmDatabase db, IEnumerable<Tagbase> res, List<FilterMessageItem> vfilters)
        {
            return res.Where((tag) => {
                var re = true;
                foreach (var vv in vfilters)
                {
                    switch (vv.Key)
                    {
                        case "keyword":
                            bool btmp = false;
                            string[] ss = vv.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            foreach (var vvv in ss)
                            {
                                btmp |= (tag.Name.Contains(vvv) || tag.Desc.Contains(vvv)||tag.CustomContent1.Contains(vvv)||tag.CustomContent2.Contains(vvv)||tag.CustomContent3.Contains(vvv));
                            }
                            re = re && btmp;
                            break;
                        case "type":
                            re = re && ((int)tag.Type == int.Parse(vv.Value));
                            break;
                        case "linktag":
                            if(tag is AlarmTag)
                            re = re && ((tag as AlarmTag).LinkTag.Contains(vv.Value));
                            break;
                    }
                }
                return re;

            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetTagMessageReply> GetTagByGroup(GetTagByGroupRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new GetTagMessageReply() { Result = false, Index = request.Index });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);

            List<TagMessage> rre = new List<TagMessage>();
            int totalpage = 0;
            int total = 0;
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    int from = request.Index * PageCount;
                    var res = db.ListAllTags().Where(e => e.Group == request.Group);

                    if (request.Filters.Count > 0)
                    {
                        res = FilterTags(db, res, request.Filters.ToList());
                    }

                    total = res.Count();

                    totalpage = total / PageCount;
                    totalpage = total % PageCount > 0 ? totalpage + 1 : totalpage;
                    int cc = 0;
                    foreach (var vv in res)
                    {
                        if (cc >= from && cc < (from + PageCount))
                        {
                            var vtag = new TagMessage() { Id = vv.Id, Name = vv.Name, Desc = vv.Desc, Group = vv.Group, TagType = (uint)vv.Type, IsEnable = vv.IsEnable, CustomContent1 = vv.CustomContent1, CustomContent2 = vv.CustomContent2, CustomContent3 = vv.CustomContent3 };
                            vtag.LinkTag = (vv is Cdy.Ant.AlarmTag) ? (vv as Cdy.Ant.AlarmTag).LinkTag : string.Empty;
                            vtag.AlarmLevel = (vv is Cdy.Ant.SimpleAlarmTag) ? (int)((vv as Cdy.Ant.SimpleAlarmTag).AlarmLevel) : 0;
                            vtag.AlarmContent = SeriseAlarmContent(vv);
                            rre.Add(vtag);
                        }
                        cc++;
                    }
                }
            }
            var msg = new GetTagMessageReply() { Result = true, Count = totalpage, Index = request.Index, TagCount = total };
            msg.Tags.AddRange(rre);
            return Task.FromResult(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<AddTagReplyMessage> AddTag(AddTagRequestMessage request, ServerCallContext context)
        {
            try
            {
                if (!CheckLoginId(request.LoginId, request.Database))
                {
                    return Task.FromResult(new AddTagReplyMessage() { Result = false });
                }
                var db = DbManager.Instance.GetDatabase(request.Database);

                if (db != null)
                {
                    lock (db)
                    {
                        DbManager.Instance.CheckAndContinueLoadDatabase(db);
                        Cdy.Ant.Tagbase tag = GetTag(request.RealTag);

                        if (tag.Id < 0)
                        {
                            db.Append(tag);
                        }
                        else
                        {
                            db.AddOrUpdate(tag);
                        }
                        
                        return Task.FromResult(new AddTagReplyMessage() { Result = true, TagId = tag.Id });
                    }
                }
                return Task.FromResult(new AddTagReplyMessage() { Result = false });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new AddTagReplyMessage() { Result = false, ErroMessage = ex.Message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> UpdateTag(UpdateTagRequestMessage request, ServerCallContext context)
        {
            try
            {
                if (!CheckLoginId(request.LoginId, request.Database))
                {
                    return Task.FromResult(new BoolResultReplay() { Result = false });
                }
                var tag = GetTag(request.Tag);
                var db = DbManager.Instance.GetDatabase(request.Database);

                if (db != null)
                {
                    lock (db)
                    {
                        DbManager.Instance.CheckAndContinueLoadDatabase(db);
                        if (db.Tags.ContainsKey(tag.Id) && tag.Id > -1)
                        {
                            db.UpdateById(tag.Id, tag);
                            return Task.FromResult(new BoolResultReplay() { Result = true });
                        }
                        else if (db.NamedTags.ContainsKey(tag.FullName))
                        {
                            db.Update(tag.FullName, tag);
                            return Task.FromResult(new BoolResultReplay() { Result = true });
                        }
                        else
                        {
                            return Task.FromResult(new BoolResultReplay() { Result = false, ErroMessage = "Tag not exist" });
                        }
                    }
                }
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new BoolResultReplay() { Result = false, ErroMessage = ex.Message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> RemoveTag(RemoveTagMessageRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {

                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    foreach (var vv in request.TagId)
                    {
                        db.Remove(vv);
                    }
                }
                return Task.FromResult(new BoolResultReplay() { Result = true });
            }
            return Task.FromResult(new BoolResultReplay() { Result = false });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> ClearTag(ClearTagRequestMessage request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }

            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    foreach (var vv in db.GetTagsByGroup(request.GroupFullName))
                    {
                        db.RemoveWithoutGroupProcess(vv);
                    }
                    var grp = db.GetGroup(request.GroupFullName);
                    if (grp != null)
                        grp.Tags.Clear();
                }
                return Task.FromResult(new BoolResultReplay() { Result = true });
            }
            return Task.FromResult(new BoolResultReplay() { Result = false });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> ClearAllTag(GetRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    db.Tags.Clear();
                    db.NamedTags.Clear();
                    db.Groups.Clear();
                }
                return Task.FromResult(new BoolResultReplay() { Result = true });
            }
            return Task.FromResult(new BoolResultReplay() { Result = false });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ResetTagIdReplay> ResetTagId(ResetTagIdRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new ResetTagIdReplay() { Result = false });
            }

            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                Dictionary<int, int> res = new Dictionary<int, int>();
                ResetTagIdReplay rep = new ResetTagIdReplay() { Result = true };
                foreach (var vv in request.TagIds.OrderBy(e => e))
                {
                    bool ishase = false;

                    int endindex = vv < request.StartId ? int.MaxValue : vv;

                    for (int i = request.StartId; i < endindex; i++)
                    {
                        if (!db.Tags.ContainsKey(i))
                        {
                            ishase = true;
                            var rtag = db.Tags[vv];
                            db.Tags.Remove(vv);

                            rtag.Id = i;
                            db.Tags.Add(i, rtag);
                            res.Add(vv, i);
                            break;
                        }
                    }
                    if (!ishase)
                        res.Add(vv, vv);
                }

                rep.TagIds.AddRange(res.Select(e => new IntKeyValueMessage() { Key = e.Key, Value = e.Value }));
                return Task.FromResult(rep);
            }
            return Task.FromResult(new ResetTagIdReplay() { Result = false });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ImportTagRequestReplyMessage> Import(ImportTagRequestMessage request, ServerCallContext context)
        {
            try
            {
                if (!CheckLoginId(request.LoginId, request.Database))
                {
                    return Task.FromResult(new ImportTagRequestReplyMessage() { Result = false });
                }
                var db = DbManager.Instance.GetDatabase(request.Database);

                if (db != null)
                {
                    lock (db)
                    {
                        DbManager.Instance.CheckAndContinueLoadDatabase(db);
                        Cdy.Ant.Tagbase tag = GetTag(request.RealTag);

                        if (request.Mode == 0)
                        {
                            //修改模式
                            if (db.NamedTags.ContainsKey(tag.FullName))
                            {
                                var vid = db.NamedTags[tag.FullName].Id;
                                if (vid == tag.Id)
                                {
                                    db.UpdateById(tag.Id, tag);
                                }
                                else
                                {
                                    tag.Id = vid;
                                    db.UpdateById(tag.Id, tag);
                                }
                            }
                            else
                            {
                                db.Append(tag);
                            }

                            
                        }
                        else if (request.Mode == 1)
                        {
                            db.Add(tag);

                           
                        }
                        else
                        {
                            //直接添加

                            if (db.NamedTags.ContainsKey(tag.FullName))
                            {
                                return Task.FromResult(new ImportTagRequestReplyMessage() { Result = false, ErroMessage = "名称重复" });
                            }
                            db.Append(tag);
                        }


                        return Task.FromResult(new ImportTagRequestReplyMessage() { Result = true, TagId = tag.Id });
                    }
                }
                return Task.FromResult(new ImportTagRequestReplyMessage() { Result = false });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ImportTagRequestReplyMessage() { Result = false, ErroMessage = ex.Message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private string SeriseAlarmContent(Cdy.Ant.Tagbase tag)
        {
            StringBuilder re = new StringBuilder();
            if(tag is Cdy.Ant.AnalogAlarmTag)
            {
                var vtag = (tag as Cdy.Ant.AnalogAlarmTag);
                if(vtag.HighHighValue!=null)
                {
                    re.Append("HH:"+ vtag.HighHighValue.ToString()+";");
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
            else if(tag is Cdy.Ant.AnalogRangeAlarmTag)
            {
                foreach(var vv in (tag as Cdy.Ant.AnalogRangeAlarmTag).Items)
                {
                    re.Append(vv.ToString() + ";");
                }
            }
            else if(tag is Cdy.Ant.DigitalAlarmTag)
            {
                var vtag = tag as Cdy.Ant.DigitalAlarmTag;
                //re.Append("Delay:" + vtag.Delay+";");
                re.Append("Value:" + vtag.Value+";");
            }
            else if(tag is Cdy.Ant.DelayDigitalAlarmTag)
            {
                var vtag = tag as Cdy.Ant.DelayDigitalAlarmTag;
                re.Append("Delay:" + vtag.Delay + ";");
                re.Append("Value:" + vtag.Value + ";");
            }
            else if(tag is Cdy.Ant.PulseAlarmTag)
            {
                var vtag = tag as Cdy.Ant.PulseAlarmTag;
                re.Append("PulseType:" + (byte)vtag.PulseType+";");
            }
            else if(tag is Cdy.Ant.StringAlarmTag)
            {
                var vtag = tag as Cdy.Ant.StringAlarmTag;
                re.Append(vtag.Value+";");
            }
            else if(tag is Cdy.Ant.ScriptTag)
            {
                var vtag = tag as Cdy.Ant.ScriptTag;
                re.Append(vtag.Expresse + "_$^cdy^$_");
                re.Append(vtag.StartTrigger.ToString()+ "_$^cdy^$_");
                re.Append(vtag.Duration + "_$^cdy^$_");
                re.Append((int)vtag.Mode + ";");
            }
            else if(tag is Cdy.Ant.OneRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.OneRangeAlarmTag;
                re.Append("IsInRangeAlarm:" + vtag.IsInRangeAlarm + ";");
                re.Append("MinValue:" + vtag.MinValue + ";");
                re.Append("MaxValue:" + vtag.MaxValue + ";");
            }
            else if(tag is Cdy.Ant.TwoRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.TwoRangeAlarmTag;
                re.Append("IsInRangeAlarm:" + vtag.IsInRangeAlarm + ";");
                StringBuilder ss = new StringBuilder();
                foreach(var vv in vtag.AlarmDatas)
                {
                    ss.Append(vv.X + "," + vv.Y + "|");
                }
                ss.Length = ss.Length > 0 ? ss.Length - 1 : ss.Length;
                re.Append("AlarmDatas:" + ss.ToString()+";");
            }
            else if (tag is Cdy.Ant.ThreeRangeAlarmTag)
            {
                var vtag = tag as Cdy.Ant.ThreeRangeAlarmTag;
                re.Append("IsInRangeAlarm:" + vtag.IsInRangeAlarm + ";");
                StringBuilder ss = new StringBuilder();
                foreach (var vv in vtag.AlarmDatas)
                {
                    ss.Append(vv.X + "," + vv.Y +"," + vv.Z + "|");
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
        /// <param name="tmsg"></param>
        /// <returns></returns>
        private Cdy.Ant.Tagbase GetTag(TagMessage tmsg)
        {
            Cdy.Ant.Tagbase re = null;
            switch (tmsg.TagType)
            {
                case (uint)(TagType.AnalogAlarm):
                    re = new Cdy.Ant.AnalogAlarmTag();
                    break;
                case (uint)(TagType.AnalogRangeAlarm):
                    re = new Cdy.Ant.AnalogRangeAlarmTag();
                    break;
                case (uint)(TagType.DelayDigitalAlarm):
                    re = new Cdy.Ant.DelayDigitalAlarmTag();
                    break;
                case (uint)(TagType.DigitalAlarm):
                    re = new Cdy.Ant.DigitalAlarmTag();
                    break;
                case (uint)(TagType.OneRange):
                    re = new Cdy.Ant.OneRangeAlarmTag();
                    break;
                case (uint)(TagType.Pulse):
                    re = new Cdy.Ant.PulseAlarmTag();
                    break;
                case (uint)(TagType.Script):
                    re = new Cdy.Ant.ScriptTag();
                    break;
                case (uint)(TagType.StringAlarm):
                    re = new Cdy.Ant.StringAlarmTag();
                    break;
                case (uint)(TagType.ThreeRange):
                    re = new Cdy.Ant.ThreeRangeAlarmTag();
                    break;
                case (uint)(TagType.TwoRange):
                    re = new Cdy.Ant.TwoRangeAlarmTag();
                    break;
                
            }
            if (re != null)
            {
                re.Name = tmsg.Name;
                if(re is AlarmTag)
                (re as AlarmTag).LinkTag = tmsg.LinkTag;
                
                if(re is SimpleAlarmTag)
                {
                    (re as SimpleAlarmTag).AlarmLevel = (AlarmLevel)tmsg.AlarmLevel;
                }
                re.Group = tmsg.Group;
                re.Desc = tmsg.Desc;
                re.Id = (int)tmsg.Id;
                re.IsEnable = tmsg.IsEnable;
                re.CustomContent1 = tmsg.CustomContent1;
                re.CustomContent2 = tmsg.CustomContent2;
                re.CustomContent3 = tmsg.CustomContent3;
                DeSeriseAlarmContent(re, tmsg.AlarmContent);
            }

            return re;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="content"></param>
        private void DeSeriseAlarmContent(Cdy.Ant.Tagbase tag,string content)
        {
            if (tag is Cdy.Ant.AnalogAlarmTag)
            {
                var vtag = (tag as Cdy.Ant.AnalogAlarmTag);
                string[] ss = content.Split(new char[] { ';' });
                if(ss.Length>0)
                {
                    foreach(var vv in ss)
                    {
                        string[] vss = vv.Split(new char[] { ':' });
                        if(vss[0]=="HH")
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
                foreach(var vvs in ss)
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
                var ccs = content.Split("_$^cdy^$_");
                var vtag = tag as Cdy.Ant.ScriptTag;
                vtag.Expresse = ccs[0];
                if (ccs.Length > 1)
                    vtag.StartTrigger = ScriptTag.LoadTriggerFromString(ccs[1]);
                if (ccs.Length > 2)
                    vtag.Duration = int.Parse(ccs[2]);
                if (ccs.Length > 3)
                    vtag.Mode = (ExecuteMode)(int.Parse(ccs[3]));
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
                            foreach(var vvp in pps)
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

        #endregion

        #region Tag group
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<AddGroupReplay> AddTagGroup(AddGroupRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new AddGroupReplay() { Result = false });
            }
            string name = request.Name;
            string parentName = request.ParentName;
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    var vtg = db.Groups.ContainsKey(request.ParentName) ? db.Groups[request.ParentName] : null;

                    int i = 1;
                    while (db.HasChildGroup(vtg, name))
                    {
                        name = request.Name + i;
                        i++;
                    }

                    string ntmp = name;

                    if (!string.IsNullOrEmpty(parentName))
                    {
                        name = parentName + "." + name;
                    }

                    db.CheckAndAddGroup(name);
                    return Task.FromResult(new AddGroupReplay() { Result = true, Group = ntmp });
                }
            }
            return Task.FromResult(new AddGroupReplay() { Result = false, ErroMessage = "database not exist!" });
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<PasteGroupReplay> PasteTagGroup(PasteGroupRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new PasteGroupReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    var tags = db.GetTagsByGroup(request.GroupFullName);
                    var vtg = db.Groups.ContainsKey(request.TargetParentName) ? db.Groups[request.TargetParentName] : null;

                    var vsg = db.Groups.ContainsKey(request.GroupFullName) ? db.Groups[request.GroupFullName] : null;

                    if (vtg == vsg)
                    {
                        return Task.FromResult(new PasteGroupReplay() { Result = false });
                    }

                    string sname = vsg.Name;
                    int i = 1;
                    while (db.HasChildGroup(vtg, sname))
                    {
                        sname = vsg.Name + i;
                        i++;
                    }

                    Cdy.Ant.TagGroup tgg = vsg != null ? new Cdy.Ant.TagGroup() { Name = sname, Parent = vtg, Description = vsg.Description } : null;

                    if (tgg == null) return Task.FromResult(new PasteGroupReplay() { Result = false });

                    tgg = db.CheckAndAddGroup(tgg.FullName);

                    foreach (var vv in tags)
                    {
                        var tmp = vv.Clone();
                        tmp.Group = tgg.FullName;
                        db.Append(tmp);
                    }
                    return Task.FromResult(new PasteGroupReplay() { Result = true, Group = sname });
                }
            }
            return Task.FromResult(new PasteGroupReplay() { Result = false, ErroMessage = "database not exist!" });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> UpdateGroupDescription(UpdateGroupDescriptionRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                DbManager.Instance.CheckAndContinueLoadDatabase(db);
                var vtg = db.Groups.ContainsKey(request.GroupName) ? db.Groups[request.GroupName] : null;
                if (vtg != null)
                {
                    vtg.Description = request.Desc;
                }
                return Task.FromResult(new BoolResultReplay() { Result = true });
            }
            return Task.FromResult(new BoolResultReplay() { Result = false, ErroMessage = "database not exist!" });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> RenameTagGroup(RenameGroupRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    var re = db.ChangeGroupName(request.OldFullName, request.NewName);
                    return Task.FromResult(new BoolResultReplay() { Result = re });
                }
            }
            return Task.FromResult(new BoolResultReplay() { Result = false, ErroMessage = "database not exist!" });
        }


        /// <summary>
        /// 删除组
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> RemoveTagGroup(RemoveGroupRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    db.RemoveGroup(request.Name);
                }
                return Task.FromResult(new BoolResultReplay() { Result = true });
            }
            return Task.FromResult(new BoolResultReplay() { Result = false, ErroMessage = "database not exist!" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolResultReplay> MoveTagGroup(MoveGroupRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new BoolResultReplay() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            if (db != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    db.ChangeGroupParent(request.Name, request.OldParentName, request.NewParentName);
                }
                return Task.FromResult(new BoolResultReplay() { Result = true });
            }
            return Task.FromResult(new BoolResultReplay() { Result = false, ErroMessage = "database not exist!" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetTagGroupMessageReply> GetTagGroup(GetRequest request, ServerCallContext context)
        {
            if (!CheckLoginId(request.LoginId, request.Database))
            {
                return Task.FromResult(new GetTagGroupMessageReply() { Result = false });
            }
            var db = DbManager.Instance.GetDatabase(request.Database);
            GetTagGroupMessageReply re = new GetTagGroupMessageReply() { Result = true };

            if (db != null  && db.Groups != null)
            {
                lock (db)
                {
                    DbManager.Instance.CheckAndContinueLoadDatabase(db);
                    foreach (var vv in db.Groups)
                    {
                        re.Group.Add(new AntDevelopServer.TagGroup() { Name = vv.Key, Parent = vv.Value.Parent != null ? vv.Value.Parent.FullName : "", Description = vv.Value.Description != null ? vv.Value.Description : "" });
                    }
                }
            }
            return Task.FromResult(re);
        }
        #endregion
    }
}
