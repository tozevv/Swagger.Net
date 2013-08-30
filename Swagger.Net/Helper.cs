using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Swagger.Net
{
    public static class Helper
    {
        private static string _serverPath;
        private static readonly Regex _regexInteger = new Regex("int|double|float|long", RegexOptions.IgnoreCase);
        private static readonly Regex _regexString = new Regex("string|byte", RegexOptions.IgnoreCase);
        private static readonly Regex _regexDateTime = new Regex("dateTime|timeStamp", RegexOptions.IgnoreCase);
        private static readonly Regex _regexBoolean = new Regex("boolean|bool", RegexOptions.IgnoreCase);
        private static readonly Regex _regexArray = new Regex("ienumerable|isortablelist", RegexOptions.IgnoreCase);
        private static readonly string[] IgnoreTypes = new[] { "integer", "string", "date-time", "void" };
       


        public static string ServerPath
        {
            get
            {
                if (_serverPath == null)
                {
                    _serverPath = HttpContext.Current.Server.MapPath("~");
                }
                return _serverPath;
            }
        }


        public static SwaggerType GetSwaggerType(Dictionary<string, TypeInfo> models, Type type)
        {
            var swaggerType = new SwaggerType();


            if (typeof(IEnumerable<object>).IsAssignableFrom(type) || type.IsArray)
            {
                swaggerType.Type = "array";
                Type arrayType;
                if (type.IsGenericType)
                {
                    arrayType = type.GetGenericArguments().First();
                    swaggerType.Items = new ItemInfo { Ref = arrayType.Name };
                }
                else
                {
                    arrayType = type.GetElementType();
                    swaggerType.Items = arrayType.Name;
                }
                TryToAddModels(models, arrayType);
            }
            else
            {
                swaggerType.Type = type.Name;
            }

            return swaggerType;
        }

        public static void TryToAddModels(Dictionary<string, TypeInfo> models, Type type)
        {
            if (models.All(m => m.Key != type.Name))
            {
                if (type.IsClass && !IgnoreTypes.Contains(type.Name.ToLower()))
                {
                    var typeInfo = new TypeInfo() { id = type.Name };

                    var modelInfoDic = new Dictionary<string, ModelInfo>();
                    foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        string modelName = propertyInfo.Name.ToLower();
                        string swaggerType = TranslateType(propertyInfo.PropertyType.Name);


                        ModelInfo modelInfo = new ModelInfo();
                        modelInfo.type = swaggerType;
                        modelInfoDic.Add(modelName, modelInfo);

                        if (propertyInfo.PropertyType.IsEnum)
                        {
                            modelInfoDic[modelName].@enum = propertyInfo.PropertyType.GetEnumNames();
                        }

                        //if (propertyInfo.PropertyType.IsClass && !IgnoreTypes.Contains(swaggerType))
                        //{
                        //    TryToAddModels(propertyInfo.PropertyType);
                        //}

                    }

                    typeInfo.properties = modelInfoDic;


                    models.Add(type.Name, typeInfo);
                }
            }
        }
        private static string TranslateType(string type)
        {
            if (_regexInteger.IsMatch(type))
                return "integer";
            if (_regexString.IsMatch(type))
                return "string";
            if (_regexDateTime.IsMatch(type))
                return "date-time";
            if (_regexBoolean.IsMatch(type))
                return "boolean";
            if (_regexArray.IsMatch(type))
                return "array";
            return type;
        }

        public static ApiSource[] GetApiSources()
        {
            var dir = ConfigurationManager.AppSettings["ApiSourceDir"] ?? Path.Combine("docs", "apiSources");
            var fullPath = Path.Combine(ServerPath, dir);
            string serverPhysicalPath = HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"];
            var sourcesList = Directory.GetDirectories(fullPath).Select(_dir =>
                {
                    var file = Directory.GetFiles(_dir, "base.json").First();
                    return new ApiSource(
                        Path.GetFileName(_dir),
                        file.Replace(serverPhysicalPath, "/").Replace(@"\", @"/")
                        );
                });

            return sourcesList.ToArray();
        }
    }
}
