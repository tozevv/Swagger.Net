using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swagger.Net
{

    public class SwaggerType
    {
        public string Name { get; set; }
        public object Items { get; set; }
    }

    public class ResponseMessage
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class TypeInfo
    {
        public string[] values;
        public string id { get; set; }
        public string description { get; set; }
        public Dictionary<string, PropInfo> properties { get; set; }
    }

    public class PropInfo
    {
        public AllowableValues allowableValues;
        public string type { get; set; }
        public string format { get; set; }
        public string description { get; set; }
        public bool required { get; set; }
        public string[] @enum { get; set; }
        public object items { get; set; }
    }

    public class AllowableValues
    {
        public string valueType { get; set; }
        public string[] values { get; set; }
    }

    public class ItemInfo
    {
        [JsonProperty(PropertyName = "$ref")]
        public string Ref { get; set; }
    }

    public class ApiSource
    {
        public ApiSource(string name, string path)
        {
            this.name = name;
            this.path = path;
        }

        public string path { get; set; }
        public string name { get; set; }
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
        public string typeJson;
        public string httpMethod { get; set; }
        public string nickname { get; set; }
        public string type { get { return _type ?? "void"; } set { _type = value; } }
        public object items { get; set; }
        public string summary { get; set; }
        public string notes { get; set; }
        public List<ResourceApiOperationParameter> parameters { get; set; }
        public string[] consumes { get { return new[] { "application/x-www-form-urlencoded", "application/json", "text/xml"  }; } }
    }

    public class ResourceApiOperationParameter
    {
        public string defaultValue { get; set; }
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


    public class ResourceListing
    {
        public string[] produces;
        public ApiSource[] apiSources;
        public string apiVersion { get; set; }
        public string swaggerVersion { get; set; }
        public string basePath { get; set; }
        public string resourcePath { get; set; }
        public List<ResourceApi> apis { get; set; }
        public ConcurrentDictionary<string, TypeInfo> models { get; set; }
    }
}
