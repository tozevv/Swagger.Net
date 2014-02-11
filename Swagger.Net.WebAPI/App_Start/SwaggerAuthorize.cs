using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        private static string GetApiKey(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            const string apiKeyEntry = "api_key";
            IEnumerable<string> apiKey;

            actionContext.Request.Headers.TryGetValues(apiKeyEntry, out apiKey);
            if (apiKey != null && apiKey.Any())
                return apiKey.First();

            var queryEntry = actionContext.Request.GetQueryNameValuePairs().FirstOrDefault(kV => kV.Key == apiKeyEntry);

            return queryEntry.Value;

        }

        bool ISwaggerAuthorization.IsDescriptionAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (this.RoleList.Contains("Admin"))
            {
                string apiKey = GetApiKey(actionContext);
                return apiKey == "admin-key";
            }
           
            return false;
        }
    }

}