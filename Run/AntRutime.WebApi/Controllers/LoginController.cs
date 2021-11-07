using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cdy.Ant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AntRutime.WebApi;
using NSwag.Annotations;
using Cdy.Ant.Tag;

namespace AntRutime.WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [OpenApiTag("登录服务", Description = "登录服务")]
    public class LoginController : ControllerBase
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("TryLogin")]
        public LoginResponse Login([FromBody] LoginUser user)
        {
            var service = ServiceLocator.Locator.Resolve<IRuntimeSecurity>();
            if (service != null)
            {
                string Token = service.Login(user.UserName, user.Password);
                return new LoginResponse() { Token = Token, Result = !string.IsNullOrEmpty(Token), LoginTime = DateTime.Now.ToString(), TimeOut = service.TimeOut };
            }
            else
            {
                return new LoginResponse() { Result = false };
            }
        }

        /// <summary>
        /// 心跳，维持用户在线
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("Hart")]
        public ResponseBase Hart([FromBody] ResponseBase token)
        {
            return new ResponseBase() { Result = ServiceLocator.Locator.Resolve<IRuntimeSecurity>().FreshUserId(token.Token) };
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("Logout")]
        public ResponseBase Logout([FromBody] ResponseBase token)
        {
            return new ResponseBase() { Result = ServiceLocator.Locator.Resolve<IRuntimeSecurity>().Logout(token.Token) };
        }

    }
}
