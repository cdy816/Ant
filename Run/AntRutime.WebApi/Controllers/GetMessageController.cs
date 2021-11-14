using Cdy.Ant;
using Cdy.Ant.Tag;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public IEnumerable<Object> GetMessage([FromBody] GetMessageRequest request)
        {
            List<object> re = new List<object>();
            foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, request.EndTime, request.Filter))
            {
                if (vv is Cdy.Ant.AlarmMessage)
                {
                    re.Add(vv as Cdy.Ant.AlarmMessage);
                }
                else
                {
                    re.Add(vv as Cdy.Ant.InfoMessage);
                }
            }
            return re;
        }

        /// <summary>
        /// 获取指定时间段的报警消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>报警消息集合</returns>
        [HttpPost("GetAlarmMessage")]
        public IEnumerable<Cdy.Ant.AlarmMessage> GetAlarmMessage([FromBody] GetMessageRequest request)
        {
            List<Cdy.Ant.AlarmMessage> re = new List<Cdy.Ant.AlarmMessage>();
            foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, request.EndTime, request.Filter))
            {
                if (vv is Cdy.Ant.AlarmMessage)
                {
                    re.Add(vv as Cdy.Ant.AlarmMessage);
                }
            }
            return re;
        }

        /// <summary>
        /// 获取指定时间段的通知消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>提示消息集合</returns>
        [HttpPost("GetInfoMessage")]
        public IEnumerable<Cdy.Ant.InfoMessage> GetInfoMessage([FromBody] GetMessageRequest request)
        {
            List<Cdy.Ant.InfoMessage> re = new List<Cdy.Ant.InfoMessage>();
            foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, request.EndTime, request.Filter))
            {
                if (vv is Cdy.Ant.InfoMessage)
                {
                    re.Add(vv as Cdy.Ant.InfoMessage);
                }
            }
            return re;
        }

        /// <summary>
        /// 获取从某时间起，到现在的所有消息
        /// 包括报警消息、提示消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>报警消息、提示消息集合</returns>
        [HttpPost("GetRecentMessage")]
        public IEnumerable<Object> GetRecentMessage([FromBody] GetRecentMessageRequest request)
        {
            List<object> re = new List<object>();
            foreach(var vv in  WebApiMessageProxy.MessageService.Query(request.StartTime, DateTime.Now, request.Filter))
            {
                if(vv is Cdy.Ant.AlarmMessage)
                {
                    re.Add(vv as Cdy.Ant.AlarmMessage);
                }
                else
                {
                    re.Add(vv as Cdy.Ant.InfoMessage);
                }
            }
            return re;
        }


        /// <summary>
        /// 获取从某时间起，到现在的所有报警消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>报警消息集合</returns>
        [HttpPost("GetRecentAlarmMessage")]
        public IEnumerable<Cdy.Ant.AlarmMessage> GetRecentAlarmMessage([FromBody] GetRecentMessageRequest request)
        {
            List<Cdy.Ant.AlarmMessage> re = new List<Cdy.Ant.AlarmMessage>();
            foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, DateTime.Now, request.Filter))
            {
                if (vv is Cdy.Ant.AlarmMessage)
                {
                    re.Add(vv as Cdy.Ant.AlarmMessage);
                }
            }
            return re;
        }


        /// <summary>
        /// 获取从某时间起，到现在的所有提示消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>提示消息集合</returns>
        [HttpPost("GetRecentInfoMessage")]
        public IEnumerable<Cdy.Ant.InfoMessage> GetRecentInfoMessage([FromBody] GetRecentMessageRequest request)
        {
            List<Cdy.Ant.InfoMessage> re = new List<Cdy.Ant.InfoMessage>();
            foreach (var vv in WebApiMessageProxy.MessageService.Query(request.StartTime, DateTime.Now, request.Filter))
            {
                if (vv is Cdy.Ant.InfoMessage)
                {
                    re.Add(vv as Cdy.Ant.InfoMessage);
                }
            }
            return re;
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
                return new ResponseBase() { Result = true,ErroMessage="Login failed!" };
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
        /// </summary>
        public List<QueryFilter> Filter { get; set; }

    }
}
