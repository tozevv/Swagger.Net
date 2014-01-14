using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Swagger.Net.WebApi.Controllers
{
    public class HomeController : ApiController
    {
        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets the swagger file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        [ActionName("swaggerfile")]
        [SwaggerIgnore]
        public HttpResponseMessage GetSwaggerFile(string filePath)
        {
            var fullPath = Path.Combine(Helper.ServerPath, filePath);
            var fileContent= File.ReadAllText(fullPath);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(fileContent);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response;
        }
    }
}
