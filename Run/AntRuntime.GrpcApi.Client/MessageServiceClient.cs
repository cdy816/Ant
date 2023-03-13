using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.GrpcApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Client
    {

        public static Client Instance = new Client();

        private AntRuntime.GrpcApi.Message.MessageClient mCurrentClient = null;

        private string mLoginId = string.Empty;

        private int mPort = 15331;

        /// <summary>
        /// 
        /// </summary>
        public bool UseTls
        {
            get;
            set;
        } = true;



        public int Port
        {
            get { return mPort; }
            set { mPort = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ErroMessage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        private AntRuntime.GrpcApi.Message.MessageClient GetServicClient(string ip)
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
                    grpcChannel = Grpc.Net.Client.GrpcChannel.ForAddress(@"https://" + ip + ":" + Port, new GrpcChannelOptions { HttpClient = httpClient });
                }
                else
                {
                    grpcChannel = Grpc.Net.Client.GrpcChannel.ForAddress(@"http://" + ip + ":" + Port, new GrpcChannelOptions { HttpClient = httpClient });
                }
                return new AntRuntime.GrpcApi.Message.MessageClient(grpcChannel);

            }
            catch (Exception ex)
            {
                ErroMessage = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public string Login(string ip, string user, string pass)
        {
            mCurrentClient = GetServicClient(ip);
            if (mCurrentClient != null)
            {
                try
                {
                    var lres = mCurrentClient.Login(new LoginRequest() { UserName = user, Password = pass });
                    if (lres != null)
                    {
                        mLoginId = lres.LoginId;
                        return lres.LoginId;
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
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
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var lres = mCurrentClient.Logout(new LogoutRequest() { Token = mLoginId });
                    if (lres != null)
                    {
                        return lres.Result;
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return false;
        }

        /// <summary>
        /// 确认消息
        /// </summary>
        /// <param name="msgIds">一组消息Id集合</param>
        /// <param name="ackContent">备注信息</param>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public bool AckMessage(IEnumerable<long> msgIds, string ackContent, string user)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new AckMessageRequest() { Token = mLoginId, AckContent = ackContent, User = user };
                    msg.MessageIds.AddRange(msgIds);

                    var lres = mCurrentClient.AckMessage(msg);
                    if (lres != null)
                    {
                        return lres.Result;
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return false;
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="msgIds">一组消息Id集合</param>
        /// <param name="deleteContent">备注信息</param>
        /// <param name="user">用户</param>
        /// <returns></returns>
        public bool DeleteMessage(IEnumerable<long> msgIds, string deleteContent, string user)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new DeleteMessageRequest() { Token = mLoginId, DeleteNote = deleteContent, User = user };
                    msg.MessageIds.AddRange(msgIds);

                    var lres = mCurrentClient.DeleteMessage(msg);
                    if (lres != null)
                    {
                        return lres.Result;
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return false;
        }

        /// <summary>
        /// 查询指定时间段发生的消息
        /// </summary>
        /// <param name="from"></param>
        /// <param name="end"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryMessage(DateTime from, DateTime end, IEnumerable<QueryFilter> filters = null)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new GetMessageRequest() { Token = mLoginId, StartTime = from.Ticks, EndTime = end.Ticks };
                    if (filters != null)
                    {
                        foreach (var vv in filters)
                        {
                            msg.Filters.Add(vv.ToString());
                        }
                    }

                    var lres = mCurrentClient.GetMessage(msg);
                    if (lres != null)
                    {
                        var remsgs = lres.Messages;
                        if (remsgs != null)
                        {
                            List<Cdy.Ant.Message> result = new List<Cdy.Ant.Message>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(Cdy.Ant.Message.LoadFromString(mg));
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取某个时间以来，至今发生的消息
        /// </summary>
        /// <param name="from"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryRecentMessage(DateTime from, IEnumerable<QueryFilter> filters = null)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new GetMessageRecentRequest() { Token = mLoginId, Time = from.Ticks };
                    if (filters != null)
                    {
                        foreach (var vv in filters)
                        {
                            msg.Filters.Add(vv.ToString());
                        }
                    }

                    var lres = mCurrentClient.GetRecentMessage(msg);
                    if (lres != null)
                    {
                        var remsgs = lres.Messages;
                        if (remsgs != null)
                        {
                            List<Cdy.Ant.Message> result = new List<Cdy.Ant.Message>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(Cdy.Ant.Message.LoadFromString(mg));
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }


        /// <summary>
        /// 查询指定时间段发生的报警消息
        /// </summary>
        /// <param name="from"></param>
        /// <param name="end"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryAlarmMessage(DateTime from, DateTime end, IEnumerable<QueryFilter> filters = null)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new GetMessageRequest() { Token = mLoginId, StartTime = from.Ticks, EndTime = end.Ticks };
                    if (filters != null)
                    {
                        foreach (var vv in filters)
                        {
                            msg.Filters.Add(vv.ToString());
                        }
                    }

                    var lres = mCurrentClient.GetAlarmMessage(msg);
                    if (lres != null)
                    {
                        var remsgs = lres.Messages;
                        if (remsgs != null)
                        {
                            List<Cdy.Ant.Message> result = new List<Cdy.Ant.Message>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(Cdy.Ant.Message.LoadFromString(mg));
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取某个时间以来，至今发生的报警消息
        /// </summary>
        /// <param name="from"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryRecentAlarmMessage(DateTime from, IEnumerable<QueryFilter> filters = null)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new GetMessageRecentRequest() { Token = mLoginId, Time = from.Ticks };
                    if (filters != null)
                    {
                        foreach (var vv in filters)
                        {
                            msg.Filters.Add(vv.ToString());
                        }
                    }

                    var lres = mCurrentClient.GetRecentAlarmMessage(msg);
                    if (lres != null)
                    {
                        var remsgs = lres.Messages;
                        if (remsgs != null)
                        {
                            List<Cdy.Ant.Message> result = new List<Cdy.Ant.Message>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(Cdy.Ant.Message.LoadFromString(mg));
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }


        /// <summary>
        /// 查询指定时间段发生的报警消息
        /// </summary>
        /// <param name="from"></param>
        /// <param name="end"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryInfoMessage(DateTime from, DateTime end, IEnumerable<QueryFilter> filters = null)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new GetMessageRequest() { Token = mLoginId, StartTime = from.Ticks, EndTime = end.Ticks };
                    if (filters != null)
                    {
                        foreach (var vv in filters)
                        {
                            msg.Filters.Add(vv.ToString());
                        }
                    }

                    var lres = mCurrentClient.GetInfoMessage(msg);
                    if (lres != null)
                    {
                        var remsgs = lres.Messages;
                        if (remsgs != null)
                        {
                            List<Cdy.Ant.Message> result = new List<Cdy.Ant.Message>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(Cdy.Ant.Message.LoadFromString(mg));
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取某个时间以来，至今发生的报警消息
        /// </summary>
        /// <param name="from"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Message> QueryRecentInfoMessage(DateTime from, IEnumerable<QueryFilter> filters = null)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    var msg = new GetMessageRecentRequest() { Token = mLoginId, Time = from.Ticks };
                    if (filters != null)
                    {
                        foreach (var vv in filters)
                        {
                            msg.Filters.Add(vv.ToString());
                        }
                    }

                    var lres = mCurrentClient.GetRecentInfoMessage(msg);
                    if (lres != null)
                    {
                        var remsgs = lres.Messages;
                        if (remsgs != null)
                        {
                            List<Cdy.Ant.Message> result = new List<Cdy.Ant.Message>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(Cdy.Ant.Message.LoadFromString(mg));
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }

        /// <summary>
        /// 枚举所有配置变量的名称
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ListAllTags()
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                   
                    var lres = mCurrentClient.ListTagName(new ListTagNameRequest() { Token = mLoginId});
                    if (lres != null)
                    {
                        var remsgs = lres.Tags;
                        if (remsgs != null)
                        {
                            List<string> result = new List<string>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(mg);
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定变量的属性配置
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Dictionary<string,string> QueryTagProperty(string tag)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    QueryTagProperyRequest req = new QueryTagProperyRequest() { Tag = tag,Token=mLoginId };
                    var lres = mCurrentClient.QueryTagPropery(req);
                    if (lres != null)
                    {
                        var remsgs = lres.Propertys;
                        if (remsgs != null)
                        {
                            Dictionary<string,string> result = new Dictionary<string, string>(remsgs.Count);
                            foreach (var mg in remsgs)
                            {
                                result.Add(mg.Key,mg.Value);
                            }
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return null;
        }

        /// <summary>
        /// 设置变量的属性
        /// </summary>
        /// <param name="tag">变量</param>
        /// <param name="propertys">属性集合</param>
        /// <returns></returns>
        public bool ModifyTagProperty(string tag,Dictionary<string,string> propertys)
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                try
                {
                    ModityTagPropertyRequest req = new ModityTagPropertyRequest() { Tag = tag, Token = mLoginId };
                    if(propertys!=null)
                    {
                        foreach (var mg in propertys)
                        {
                            req.Propertys.Add(new KeyValue() { Key= mg.Key, Value = mg.Value });
                        }
                    }
                    var lres = mCurrentClient.ModityTagProperty(req);
                    if (lres != null)
                    {
                        return lres.Result;
                    }
                }
                catch (Exception ex)
                {
                    ErroMessage = ex.Message;
                }
            }
            return false;
        }

        public delegate void MessageNotifyCallBackDelegate(Cdy.Ant.Message callback,bool isCanceled);

        /// <summary>
        /// 新消息回调
        /// </summary>
        public  MessageNotifyCallBackDelegate MessageNotifyCallBack { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool RegistorMessageNotify()
        {
            if (mCurrentClient != null && !string.IsNullOrEmpty(mLoginId))
            {
                RegistorAlarmNotifyRequest rr = new RegistorAlarmNotifyRequest() { Token = mLoginId };
                res = mCurrentClient.RegistorAlarmNotify(rr);
                if (res != null)
                {
                    Task.Run(MessageReceivePro);
                    return true;
                }
            }
            return false;
        }

        AsyncServerStreamingCall<GetMessageResponse> res;
        private async void MessageReceivePro()
        {
            while(res!=null)
            {
                try
                {
                    await res.ResponseStream.MoveNext();
                    foreach (var vv in res.ResponseStream.Current.Messages)
                    {
                        MessageNotifyCallBack?.Invoke(Cdy.Ant.Message.LoadFromString(vv),false);
                    }
                }
                catch
                {
                    MessageNotifyCallBack?.Invoke(null, true);
                    return;
                }
            }
        }

    }

   


/// <summary>
/// 过滤条件
/// </summary>
public struct QueryFilter
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 类型，0:大于,1:小于,2:等于,3:字符串包含
        /// </summary>
        public FilterOperate Opetate { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ope"></param>
        /// <returns></returns>
        private string GetOpeString(FilterOperate ope)
        {
            switch (ope)
            {
                case FilterOperate.Equals:
                    return "==";
                case FilterOperate.Contains:
                    return "..";
                case FilterOperate.Great:
                    return ">";
                case FilterOperate.Low:
                    return "<";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return PropertyName + GetOpeString(Opetate) + Value;
        }
    }

    /// <summary>
    /// 过滤操作类型，0:大于,1:小于,2:等于,3:字符串包含
    /// </summary>
    public enum FilterOperate
    {
        /// <summary>
        /// 大于
        /// </summary>
        Great,
        /// <summary>
        /// 小于
        /// </summary>
        Low,
        /// <summary>
        /// 等于
        /// </summary>
        Equals,
        /// <summary>
        /// 包含
        /// </summary>
        Contains
    }
}
