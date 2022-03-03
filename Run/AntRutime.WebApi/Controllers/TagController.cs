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
    [OpenApiTag("变量服务", Description = "变量服务")]
    public class TagController : ControllerBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public ListTagsResult ListTags([FromBody] TagRequestBase request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token) && service.IsAdmin(request.Token))
            {
                var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ListTagNames();
                return new ListTagsResult() { Result = true, Tags = re };
            }
            return new ListTagsResult() { Result = false, ErroMessage = "Login failed!" };
        }

        /// <summary>
        /// 修改变量配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("ModifyTag")]
        public ResponseBase ModifyTag([FromBody] ModifyTagRequest request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token) && service.IsAdmin(request.Token))
            {
               var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ModifyTag(request.TagName, request.PropertyValues);
                return new ResponseBase() { Result = true };
            }
            return new ResponseBase() { Result = false, ErroMessage = "Login failed!" };
        }

        /// <summary>
        /// 获取变量的可供修改的属性配置
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("QueryTagProperty")]
        public QueryTagPropertyResponse QueryTagProperty([FromBody] TagRequestBase request)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service.CheckLogin(request.Token) && service.IsAdmin(request.Token))
            {
                var re = ServiceLocator.Locator.Resolve<IRuntimeTagService>().ListTagDefines(request.TagName);
                return new QueryTagPropertyResponse() { Result = true,PropertyValues=re };
            }
            return new QueryTagPropertyResponse() { Result = false,ErroMessage="Login failed!" };
        }


    }

    /// <summary>
    /// 变量服务请求基类
    /// </summary>
    public class TagRequestBase:RequestBase
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        public string TagName { get; set; }
    }

    /// <summary>
    /// 修改变量配置请求
    /// </summary>
    public class ModifyTagRequest : TagRequestBase
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 属性名称、值的集合
        /// </summary>
        public Dictionary<string, string> PropertyValues { get; set; }
        #endregion ...Properties...

        #region ... Methods    ...

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 变量可供修改属性的集合
    /// </summary>
    public class QueryTagPropertyResponse:ResponseBase
    {
        /// <summary>
        /// 属性、值集合
        /// </summary>
        public Dictionary<string, string> PropertyValues { get; set; }
    }

    /// <summary>
    /// 枚举变量返回
    /// </summary>
    public class ListTagsResult:ResponseBase
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Tags { get; set; }
    }

}
