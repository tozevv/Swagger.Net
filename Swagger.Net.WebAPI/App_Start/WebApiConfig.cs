using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Swagger.Net.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            config.Routes.MapHttpRoute(
               name: "SwaggerFile",
               routeTemplate: "home/swaggerfile",
               defaults: new { controller = "Home", action = "swaggerfile" }
           );

            config.Routes.MapHttpRoute(
                name: "ApiRoutes",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

        }
    }
}
