using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AntRutime.WebApi.Controllers
{

    /// <summary>
    /// 
    /// </summary>
    //[Route("[controller]")]
    public class HomeController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        public void Index()
        {
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Redirect("./swagger/index.html");
            base.OnActionExecuting(context);
        }

    }
}
