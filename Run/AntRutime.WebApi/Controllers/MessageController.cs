using Cdy.Ant.Tag;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntRutime;
using Cdy.Ant;

namespace AntRutime.WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [OpenApiTag("消息服务", Description = "消息服务")]
    public class MessageController : ControllerBase
    {
        /// <summary>
        /// 获取指定时间段的消息
        /// 包括报警消息、提示消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>报警消息、提示消息集合</returns>
        [HttpPost("GetMessage")]
        public MessageResponse GetMessage([FromBody] GetMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<object> re = new List<object>();
                foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, request.EndTime, request.Filter.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.AlarmMessage)
                    {
                        re.Add(vv as Cdy.Ant.Tag.AlarmMessage);
                    }
                    else
                    {
                        re.Add(vv as Cdy.Ant.Tag.InfoMessage);
                    }
                }
                return new MessageResponse() { Result = true, Message = re };
            }
            else
            {
                return new MessageResponse() { Result = false, ErroMessage = "Login failed!" };
            }
        }

        /// <summary>
        /// 获取指定时间段的报警消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>报警消息集合</returns>
        [HttpPost("GetAlarmMessage")]
        public AlarmMessageResponse GetAlarmMessage([FromBody] GetMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<Cdy.Ant.Tag.AlarmMessage> re = new List<Cdy.Ant.Tag.AlarmMessage>();
                foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, request.EndTime, request.Filter.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.AlarmMessage)
                    {
                        re.Add(vv as Cdy.Ant.Tag.AlarmMessage);
                    }
                }
                return new AlarmMessageResponse() { Result = true, Message = re };
            }
            else
            {
                return new AlarmMessageResponse() { Result = false, ErroMessage = "Login failed!" };
            }
        }

        /// <summary>
        /// 获取指定时间段的通知消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>提示消息集合</returns>
        [HttpPost("GetInfoMessage")]
        public InfoMessageResponse GetInfoMessage([FromBody] GetMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<Cdy.Ant.Tag.InfoMessage> re = new List<Cdy.Ant.Tag.InfoMessage>();
                foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, request.EndTime, request.Filter.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.InfoMessage)
                    {
                        re.Add(vv as Cdy.Ant.Tag.InfoMessage);
                    }
                }
                return new InfoMessageResponse() { Result = true, Message = re };
            }
            else
            {
                return new InfoMessageResponse() { Result = false, ErroMessage = "Login failed!" };
            }
        }

        /// <summary>
        /// 获取从某时间起，到现在的所有消息
        /// 包括报警消息、提示消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>报警消息、提示消息集合</returns>
        [HttpPost("GetRecentMessage")]
        public MessageResponse GetRecentMessage([FromBody] GetRecentMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<object> re = new List<object>();
                foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, DateTime.Now, request.Filter.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.AlarmMessage)
                    {
                        re.Add(vv as Cdy.Ant.Tag.AlarmMessage);
                    }
                    else
                    {
                        re.Add(vv as Cdy.Ant.Tag.InfoMessage);
                    }
                }
                return new MessageResponse() { Result = true, Message = re };
            }
            else
            {
                return new MessageResponse() { Result = false, ErroMessage = "Login failed!" };
            }
        }


        /// <summary>
        /// 获取从某时间起，到现在的所有报警消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>报警消息集合</returns>
        [HttpPost("GetRecentAlarmMessage")]
        public AlarmMessageResponse GetRecentAlarmMessage([FromBody] GetRecentMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<Cdy.Ant.Tag.AlarmMessage> re = new List<Cdy.Ant.Tag.AlarmMessage>();
                foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, DateTime.Now, request.Filter.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.AlarmMessage)
                    {
                        re.Add(vv as Cdy.Ant.Tag.AlarmMessage);
                    }
                }
                return new AlarmMessageResponse() { Result = true, Message = re };
            }
            else
            {
                return new AlarmMessageResponse() { Result = false, ErroMessage = "Login failed!" };
            }
        }


        /// <summary>
        /// 获取从某时间起，到现在的所有提示消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>提示消息集合</returns>
        [HttpPost("GetRecentInfoMessage")]
        public InfoMessageResponse GetRecentInfoMessage([FromBody] GetRecentMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token))
            {
                List<Cdy.Ant.Tag.InfoMessage> re = new List<Cdy.Ant.Tag.InfoMessage>();
                foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, DateTime.Now, request.Filter.GetFiltersFromString()))
                {
                    if (vv is Cdy.Ant.Tag.InfoMessage)
                    {
                        re.Add(vv as Cdy.Ant.Tag.InfoMessage);
                    }
                }
                return new InfoMessageResponse() { Result = true, Message = re };
            }
            else
            {
                return new InfoMessageResponse() { Result = false, ErroMessage = "Login failed!" };
            }
        }

        /// <summary>
        /// 确认消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("AckMessage")]
        public ResponseBase AckMessage([FromBody] AckMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if(service.CheckLogin(request.Token))
            {
                WebApiMessageProxy.MessageService.AckMessage(request.Id, request.AckContent, request.User);
                return new ResponseBase() { Result = true };
            }
            else
            {
                return new ResponseBase() { Result = false,ErroMessage="Login failed!" };
            }
           
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("DeleteMessage")]
        public ResponseBase DeleteMessage([FromBody] DeleteMessageRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token)&& service.IsAdmin(request.Token))
            {
                WebApiMessageProxy.MessageService.DeleteMessage(request.Id, request.DeleteNote, request.User);
                return new ResponseBase() { Result = true };
            }
            else
            {
                return new ResponseBase() { Result = false, ErroMessage = "Login failed!" };
            }

        }
    }

    /// <summary>
    /// 消息确认
    /// </summary>
    public class AckMessageRequest : RequestBase
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 确认内容
        /// </summary>
        public string AckContent { get; set; }

        /// <summary>
        /// 确认人
        /// </summary>
        public string User { get; set; }
    }


    /// <summary>
    /// 删除消息
    /// </summary>
    public class DeleteMessageRequest : RequestBase
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 删除备注
        /// </summary>
        public string DeleteNote { get; set; }

        /// <summary>
        /// 删除人
        /// </summary>
        public string User { get; set; }
    }


    /// <summary>
    /// 获取指定时间段的消息 查询定义
    /// </summary>
    public class GetMessageRequest: GetRecentMessageRequest
    {

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// 获取从某时间起，到现在的所有报警消息 查询定义
    /// </summary>
    public class GetRecentMessageRequest:RequestBase
    {

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 过滤条件
        /// name==value  等于
        /// name>value  大于
        /// name &lt; value  小于
        /// name..value  包含
        /// </summary>
        public List<string> Filter { get; set; }

    }

    /// <summary>
    /// 消息返回结果
    /// </summary>
    public class MessageResponse : ResponseBase
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public IEnumerable<Object> Message { get; set; }
    }

    /// <summary>
    /// 报警消息返回结果
    /// </summary>
    public class AlarmMessageResponse : ResponseBase
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public IEnumerable<AlarmMessage> Message { get; set; }
    }

    /// <summary>
    /// 提示消息返回结果
    /// </summary>
    public class InfoMessageResponse : ResponseBase
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        public IEnumerable<InfoMessage> Message { get; set; }
    }
}
