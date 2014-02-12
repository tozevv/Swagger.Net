using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Routing;
using Swagger.Net;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Swagger.Net.WebApi.App_Start.SwaggerNet), "PreStart")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(Swagger.Net.WebApi.App_Start.SwaggerNet), "PostStart")]
namespace Swagger.Net.WebApi.App_Start
{
    public static class SwaggerNet
    {
        public static void PreStart()
        {
            SwaggerGen.LowercaseRoutes = true;
            SwaggerGen.IgnoreRouteQueryParameters = true;
            RouteTable.Routes.MapHttpRoute(
                name: "SwaggerApiBase",
                routeTemplate: "api/swagger",
                defaults: new { Controller = "Swagger" }
                );

            RouteTable.Routes.MapHttpRoute(
                           name: "SwaggerApiTags",
                           routeTemplate: "api/docs/tags",
                           defaults: new { swagger = true, controller = "tags", action = "Get" }
                           );

            RouteTable.Routes.MapHttpRoute(
                            name: "SwaggerApiUserDetails",
                            routeTemplate: "api/docs/home",
                            defaults: new { swagger = true, controller = "home" , action = "Get" }
                            );

        }

        public static void PostStart()
        {
            var config = GlobalConfiguration.Configuration;

            config.Filters.Add(new SwaggerActionFilter());

            try
            {
                var binFolder = HostingEnvironment.MapPath("~/bin/");
                config.Services.Replace(typeof(IDocumentationProvider),
                    new XmlCommentDocumentationProvider(Directory.GetFiles(binFolder, "Swagger.Net.*.xml")));
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Please enable \"XML documentation file\" in project properties with default (bin\\Swagger.Net.WebApi.XML) value or edit value in App_Start\\SwaggerNet.cs");
            }
        }
    }
}