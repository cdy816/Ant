using AntRuntime.GrpcApi;
using Cdy.Ant;
using Cdy.Ant.Tag;
using Grpc.Core;

namespace AntRuntime.GrpcApi.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageService : Message.MessageBase
    {
        private readonly ILogger<MessageService> _logger;
        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service != null)
            {
                string Token = service.Login(request.UserName, request.Password);
                return Task.FromResult<LoginReply>(new LoginReply() { LoginId = Token, Result = true });
            }
            else
            {
                return Task.FromResult<LoginReply>(new LoginReply() { Result = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolMessageReply> Logout(LogoutRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service != null)
            {
                 service.Logout(request.Token);
                return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result=true });
            }
            return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result = false });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolMessageReply> AckMessage(AckMessageRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                foreach(var vv in request.MessageIds)
                GrpcApiMessageProxy.MessageService.AckMessage(vv, request.AckContent, request.User);
                return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result = true });
            }
            else
            {
                return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result = false });
            }
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolMessageReply> DeleteMessage(DeleteMessageRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                foreach (var vv in request.MessageIds)
                    GrpcApiMessageProxy.MessageService.DeleteMessage(vv, request.DeleteNote, request.User);
                return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result = true });
            }
            else
            {
                return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result = false });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetMessageResponse> GetAlarmMessage(GetMessageRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<string> re = new List<string>();

                List<string> filters = new List<string>();
                if (request.Filters.Count > 0)
                {
                    filters.AddRange(request.Filters);
                }

                foreach (var vv in GrpcApiMessageProxy.MessageService.Query(DateTime.FromBinary(request.StartTime), DateTime.FromBinary(request.EndTime), filters.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.AlarmMessage)
                    {
                        re.Add(vv.ToString());
                    }
                }
                var gm = new GetMessageResponse();
                gm.Messages.AddRange(re);

                return Task.FromResult(gm);
            }
            else
            {
                return Task.FromResult(new GetMessageResponse());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetMessageResponse> GetInfoMessage(GetMessageRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<string> re = new List<string>();

                List<string> filters = new List<string>();
                if (request.Filters.Count > 0)
                {
                    filters.AddRange(request.Filters);
                }

                foreach (var vv in GrpcApiMessageProxy.MessageService.Query(DateTime.FromBinary(request.StartTime), DateTime.FromBinary(request.EndTime), filters.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.InfoMessage)
                    {
                        re.Add(vv.ToString());
                    }
                }
                var gm = new GetMessageResponse();
                gm.Messages.AddRange(re);

                return Task.FromResult(gm);
            }
            else
            {
                return Task.FromResult(new GetMessageResponse());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetMessageResponse> GetMessage(GetMessageRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<string> re = new List<string>();

                List<string> filters = new List<string>();
                if(request.Filters.Count > 0)
                {
                    filters.AddRange(request.Filters);
                }

                foreach (var vv in GrpcApiMessageProxy.MessageService.Query(DateTime.FromBinary(request.StartTime),DateTime.FromBinary(request.EndTime), filters.GetFiltersFromString()))
                {
                    re.Add(vv.ToString());
                }
                var gm = new GetMessageResponse();
                gm.Messages.AddRange(re);

                return Task.FromResult(gm);
            }
            else
            {
                return Task.FromResult(new GetMessageResponse());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetMessageResponse> GetRecentMessage(GetMessageRecentRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<string> re = new List<string>();

                List<string> filters = new List<string>();
                if (request.Filters.Count > 0)
                {
                    filters.AddRange(request.Filters);
                }

                foreach (var vv in GrpcApiMessageProxy.MessageService.Query(DateTime.FromBinary(request.Time), DateTime.Now, filters.GetFiltersFromString()))
                {
                    re.Add(vv.ToString());
                }
                var gm = new GetMessageResponse();
                gm.Messages.AddRange(re);

                return Task.FromResult(gm);
            }
            else
            {
                return Task.FromResult(new GetMessageResponse());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetMessageResponse> GetRecentInfoMessage(GetMessageRecentRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<string> re = new List<string>();

                List<string> filters = new List<string>();
                if (request.Filters.Count > 0)
                {
                    filters.AddRange(request.Filters);
                }

                foreach (var vv in GrpcApiMessageProxy.MessageService.Query(DateTime.FromBinary(request.Time), DateTime.Now, filters.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.InfoMessage)
                    {
                        re.Add(vv.ToString());
                    }
                }
                var gm = new GetMessageResponse();
                gm.Messages.AddRange(re);

                return Task.FromResult(gm);
            }
            else
            {
                return Task.FromResult(new GetMessageResponse());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<GetMessageResponse> GetRecentAlarmMessage(GetMessageRecentRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<string> re = new List<string>();

                List<string> filters = new List<string>();
                if (request.Filters.Count > 0)
                {
                    filters.AddRange(request.Filters);
                }

                foreach (var vv in GrpcApiMessageProxy.MessageService.Query(DateTime.FromBinary(request.Time), DateTime.Now, filters.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.AlarmMessage)
                    {
                        re.Add(vv.ToString());
                    }
                }
                var gm = new GetMessageResponse();
                gm.Messages.AddRange(re);

                return Task.FromResult(gm);
            }
            else
            {
                return Task.FromResult(new GetMessageResponse());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<ListTagNameReply> ListTagName(ListTagNameRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token) && service.IsAdmin(request.Token))
            {
                ListTagNameReply re = new ListTagNameReply();
                re.Tags.AddRange(ServiceLocator.Locator.Resolve<IRuntimeTagService>().ListTagNames());
                return Task.FromResult<ListTagNameReply>(re);
            }
            return Task.FromResult<ListTagNameReply>(new ListTagNameReply());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<BoolMessageReply> ModityTagProperty(ModityTagPropertyRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token) && service.IsAdmin(request.Token))
            {
                Dictionary<string, string> dtmp = new Dictionary<string, string>();
                foreach(var vv in request.Propertys)
                {
                    dtmp.Add(vv.Key, vv.Value);
                    
                }
                var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ModifyTag(request.Tag, dtmp);
                return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result = re });
            }
            return Task.FromResult<BoolMessageReply>(new BoolMessageReply() { Result = false });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<QueryTagProperyReply> QueryTagPropery(QueryTagProperyRequest request, ServerCallContext context)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token) && service.IsAdmin(request.Token))
            {
                var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ListTagDefines(request.Tag);
                QueryTagProperyReply res = new QueryTagProperyReply();
                foreach(var vv in re)
                {
                    res.Propertys.Add(new KeyValue() { Key=vv.Key, Value=vv.Value });
                }
                return Task.FromResult<QueryTagProperyReply>(res);
            }
            return   Task.FromResult<QueryTagProperyReply>(new QueryTagProperyReply());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task RegistorAlarmNotify(RegistorAlarmNotifyRequest request, IServerStreamWriter<GetMessageResponse> responseStream, ServerCallContext context)
        {
            return Task.Run(()=> {
                var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
                if (service.CheckLogin(request.Token) && service.IsAdmin(request.Token))
                {
                    string skey = context.Peer;
                    bool iscancel = false;
                    MessageHelper.Helper.RegistorMessageSend(skey, responseStream, () => {
                        iscancel = true;
                    });
                    while (!iscancel) ;
                }
            });
        }

    }
}