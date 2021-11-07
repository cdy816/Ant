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
    [Route("api/[controller]")]
    [ApiController]
    [OpenApiTag("获取消息", Description = "获取报警消息")]
    public class GetMessageController : ControllerBase
    {
        /// <summary>
        /// 获取指定时间段的报警
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<Cdy.Ant.Message> GetMessage([FromBody] GetMessageRequest request)
        {
            return WebApiMessageProxy.MessageService.Query(request.StartTime,request.EndTime,request.Filter);
        }

        /// <summary>
        /// 获取从某时间起，到现在的所有消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<Cdy.Ant.Message> GetRecentMessage([FromBody] GetRecentMessageRequest request)
        {
            return WebApiMessageProxy.MessageService.Query(request.StartTime,DateTime.Now, request.Filter);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetMessageRequest: GetRecentMessageRequest
    {

        /// <summary>
        /// 
        /// </summary>
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GetRecentMessageRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string LoginId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 过滤条件
        /// </summary>
        public List<QueryFilter> Filter { get; set; }

    }
}
