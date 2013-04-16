using System;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Swagger.Net.WebApi
{
    /// <summary>
    /// Sample swagger authorize attribute
    /// Will only include in swagger API's with a valid key
    /// </summary>
    public class SwaggerAuthorize : AuthorizeAttribute, ISwaggerAuthorization
    {
        private string[] RoleList
        {
            get
            {
                return this.Roles.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        bool ISwaggerAuthorization.IsDescriptionAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (this.RoleList.Contains("Admin"))
            {
                HttpContextBase httpContextBase = actionContext.Request.Properties["MS_HttpContext"] as HttpContextBase;
                string apiKey = httpContextBase.Request.Params["api_key"];
                return apiKey == "admin-key";
            }
            else
            {
                return false;
            }
        }
    }

}