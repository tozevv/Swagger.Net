using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace Swagger.Net
{
    public static class SwaggerGen
    {

        /// <summary>
        /// If true routes will be lowercased
        /// </summary>
        public static bool LowercaseRoutes { get; set; }

        /// <summary>
        /// If true route paramters after "?" will be ignored, since they are not properly supported by SwaggerUI
        /// </summary>
        public static bool IgnoreRouteQueryParameters { get; set; }


        public const string SWAGGER = "swagger";
        public const string SWAGGER_VERSION = "2.0";
        public const string FROMURI = "FromUri";
        public const string FROMBODY = "FromBody";
        public const string QUERY = "query";
        public const string PATH = "path";
        public const string BODY = "body";
        private static readonly Dictionary<string, TypeInfo> models = new Dictionary<string, TypeInfo>();
        private static readonly string[] IgnoreTypes = new[] { "integer", "string", "date-time", "void" };

        public static string CreatePath(string path) 
        {
            if (path != null)
            {
                if (LowercaseRoutes)
                {
                    path = path.ToLower();
                }
                if (IgnoreRouteQueryParameters)
                {
                    int splitIndex = path.IndexOf('?');
                    if (splitIndex > -1) path = path.Substring(0, splitIndex);
                }
            }
            return path;
        }

        /// <summary>
        /// Create a resource listing
        /// </summary>
        /// <param name="actionContext">Current action context</param>
        /// <param name="includeResourcePath">Should the resource path property be included in the response</param>
        /// <returns>A resource Listing</returns>
        public static ResourceListing CreateResourceListing(HttpActionContext actionContext, bool includeResourcePath = true)
        {
            return CreateResourceListing(actionContext.ControllerContext, includeResourcePath);
        }

        /// <summary>
        /// Create a resource listing
        /// </summary>
        /// <param name="actionContext">Current controller context</param>
        /// <param name="includeResourcePath">Should the resource path property be included in the response</param>
        /// <returns>A resrouce listing</returns>
        public static ResourceListing CreateResourceListing(HttpControllerContext controllerContext, bool includeResourcePath = false)
        {
            Uri uri = controllerContext.Request.RequestUri;

            ResourceListing rl = new ResourceListing()
            {
                apiVersion = Assembly.GetCallingAssembly().GetType().Assembly.GetName().Version.ToString(),
                swaggerVersion = SWAGGER_VERSION,
                basePath = uri.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath.TrimEnd('/'),
                apis = new List<ResourceApi>(),
                models = models,
                produces = new[] { "application/json", "application/xml" }

            };

            if (includeResourcePath) rl.resourcePath = controllerContext.ControllerDescriptor.ControllerName;

            return rl;
        }

        /// <summary>
        /// Create an api element 
        /// </summary>
        /// <param name="api">Description of the api via the ApiExplorer</param>
        /// <returns>A resource api</returns>
        public static ResourceApi CreateResourceApi(ApiDescription api)
        {
            ResourceApi rApi = new ResourceApi()
            {
                path = "/" + api.RelativePath,
                description = api.Documentation,
                operations = new List<ResourceApiOperation>()
            };

            rApi.path = CreatePath(rApi.path);
            return rApi;

          
        }

        /// <summary>
        /// Creates an api operation
        /// </summary>
        /// <param name="api">Description of the api via the ApiExplorer</param>
        /// <param name="docProvider">Access to the XML docs written in code</param>
        /// <returns>An api operation</returns>
        public static ResourceApiOperation CreateResourceApiOperation(ApiDescription api, XmlCommentDocumentationProvider docProvider)
        {
            ResponseType responseType = docProvider.GetResponseType(api.ActionDescriptor);
            ResourceApiOperation rApiOperation = new ResourceApiOperation()
            {
                httpMethod = api.HttpMethod.ToString(),
                nickname = docProvider.GetNickname(api.ActionDescriptor),
                type = responseType.Name,
                summary = api.Documentation,
                notes = docProvider.GetNotes(api.ActionDescriptor),
                parameters = new List<ResourceApiOperationParameter>(),
                responseMessages = docProvider.GetResponseCodes(api.ActionDescriptor)
            };

            TryToAddModels(responseType.Type);

            return rApiOperation;
        }

        private static void TryToAddModels(Type type)
        {
            if (models.All(m => m.Key != type.Name))
            {
                if (type.IsClass && !IgnoreTypes.Contains(type.Name.ToLower()))
                {
                    var typeInfo = new TypeInfo() { id = type.Name };

                    var modelInfoDic = new Dictionary<string, ModelInfo>();
                    foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var swaggerType = TranslateType(propertyInfo.PropertyType.Name);
                        modelInfoDic.Add(propertyInfo.Name.ToLower(), new ModelInfo() { type = swaggerType });

                        if (propertyInfo.PropertyType.IsEnum)
                        {
                            modelInfoDic[propertyInfo.Name.ToLower()].@enum = propertyInfo.PropertyType.GetEnumNames();
                        }

                        if (propertyInfo.PropertyType.IsClass && !IgnoreTypes.Contains(swaggerType))
                        {
                            TryToAddModels(propertyInfo.PropertyType);
                        }

                    }

                    typeInfo.properties = modelInfoDic;


                    models.Add(type.Name, typeInfo);
                }
            }
        }

        private static string TranslateType(string type)
        {

            if (new Regex("int|double|float|long", RegexOptions.IgnoreCase).IsMatch(type))
                return "integer";
            if (new Regex("string|byte", RegexOptions.IgnoreCase).IsMatch(type))
                return "string";
            if (new Regex("dateTime|timeStamp", RegexOptions.IgnoreCase).IsMatch(type))
                return "date-time";

            return type;
        }


        /// <summary>
        /// Creates an operation parameter
        /// </summary>
        /// <param name="api">Description of the api via the ApiExplorer</param>
        /// <param name="param">Description of a parameter on an operation via the ApiExplorer</param>
        /// <param name="docProvider">Access to the XML docs written in code</param>
        /// <returns>An operation parameter</returns>
        public static ResourceApiOperationParameter CreateResourceApiOperationParameter(ApiDescription api, ApiParameterDescription param, XmlCommentDocumentationProvider docProvider)
        {
            string paramType = (param.Source.ToString().Equals(FROMURI)) ? QUERY : BODY;
            ResourceApiOperationParameter parameter = new ResourceApiOperationParameter()
            {
                paramType = (paramType == "query" && CreatePath(api.RelativePath).IndexOf("{" + param.Name + "}") > -1) ? PATH : paramType,
                name = param.Name,
                description = param.Documentation,
                dataType = param.ParameterDescriptor.ParameterType.Name,
                required = docProvider.GetRequired(param.ParameterDescriptor),
                @enum = docProvider.GetPossibleValues(param.ParameterDescriptor)
            };

            return parameter;
        }
    }

    public class ResponseMessage
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class TypeInfo
    {
        public string id { get; set; }
        public Dictionary<string, ModelInfo> properties { get; set; }
    }

    public class ModelInfo
    {
        public string type { get; set; }
        public string format { get; set; }
        public string description { get; set; }
        public IEnumerable<string> required { get; set; }
        public string[] @enum { get; set; }
    }



    public class ResourceListing
    {
        public string[] produces;
        public string apiVersion { get; set; }
        public string swaggerVersion { get; set; }
        public string basePath { get; set; }
        public string resourcePath { get; set; }
        public List<ResourceApi> apis { get; set; }
        public Dictionary<string, TypeInfo> models { get; set; }
    }

    public class ResourceApi
    {
        public string path { get; set; }
        public string description { get; set; }
        public List<ResourceApiOperation> operations { get; set; }
    }

    public class ResourceApiOperation
    {
        private string _type;
        public List<ResponseMessage> responseMessages;
        public string httpMethod { get; set; }
        public string nickname { get; set; }
        public string type { get { return _type ?? "void"; } set { _type = value; } }
        public string summary { get; set; }
        public string notes { get; set; }
        public List<ResourceApiOperationParameter> parameters { get; set; }
    }

    public class ResourceApiOperationParameter
    {
        public string paramType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string dataType { get; set; }
        public bool required { get; set; }
        public bool allowMultiple { get; set; }
        public OperationParameterAllowableValues allowableValues { get; set; }

        public string[] @enum { get; set; }
    }

    public class OperationParameterAllowableValues
    {
        public int max { get; set; }
        public int min { get; set; }
        public string valueType { get; set; }
    }
}