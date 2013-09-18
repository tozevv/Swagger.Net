using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Swagger.Net.WebApi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            

            routes.MapHttpRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Get", id = RouteParameter.Optional }
            );
        }
    }
}