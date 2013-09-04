using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Http.Controllers;

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
        private static readonly string[] IgnoreTypes = new[] { "integer", "string", "date-time", "void", "object" };
        private static string[] _assembliesToExpose;

        private static string[] AssembliesToExpose
        {
            get
            {
                if (_assembliesToExpose == null)
                {
                    var assembliesToExpose = ConfigurationManager.AppSettings["SwaggerAssembliesToExpose"];
                    if (assembliesToExpose != null)
                        Helper._assembliesToExpose = assembliesToExpose.Split(',');
                }
                return _assembliesToExpose;
            }
        }
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


        public static SwaggerType GetSwaggerType(Type type)
        {
            var swaggerType = new SwaggerType();
            if (typeof(IEnumerable<object>).IsAssignableFrom(type) || type.IsArray)
            {
                swaggerType.Name = "array";
                Type arrayType;
                if (type.IsGenericType)
                {
                    arrayType = type.GetGenericArguments().First();
                }
                else
                {
                    arrayType = type.GetElementType();

                }
                swaggerType.Items = new ItemInfo { Ref = GetTypeName(arrayType) };
            }
            else
            {
                swaggerType.Name = GetTypeName(type);
            }

            return swaggerType;
        }

        public static void TryToAddModels(ConcurrentDictionary<string, TypeInfo> models, Type type, XmlCommentDocumentationProvider docProvider, ConcurrentDictionary<string, string> typesToReturn = null, int level = 0)
        {
            var _type = type;
            if (type.IsArray)
                _type = type.GetElementType();
            else if (type.IsGenericType)
                _type = type.GetGenericArguments().First();

            string typeName = GetTypeName(_type);

            if (models.All(m => m.Key != typeName))
            {
                if (IsOutputable(_type))
                {
                    var typeInfo = new TypeInfo() { id = typeName };
                    if (!IgnoreTypes.Contains(_type.Name.ToLower()))
                    {
                        typeInfo.description = docProvider.GetSummary(_type);
                    }

                    var modelInfoDic = new Dictionary<string, PropInfo>();
                    foreach (var propertyInfo in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var propInfo = new PropInfo();

                        string propName = propertyInfo.Name;

                        SwaggerType swaggerType = Helper.GetSwaggerType(propertyInfo.PropertyType);
                        propInfo.type = swaggerType.Name;
                        propInfo.items = swaggerType.Items;
                        propInfo.required = IsRequired(propertyInfo, docProvider);

                        if (!modelInfoDic.Keys.Contains(propName))
                            modelInfoDic.Add(propName, propInfo);

                        if (!IgnoreTypes.Contains(propInfo.type))
                        {
                            propInfo.description = docProvider.GetSummary(propertyInfo);


                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                modelInfoDic[propName].@enum = propertyInfo.PropertyType.GetEnumNames();
                            }

                            if (IsOutputable(propertyInfo.PropertyType) && level < 10 && !propertyInfo.PropertyType.Assembly.GetName().Name.Contains("System"))
                            {
                                TryToAddModels(models, propertyInfo.PropertyType, docProvider, typesToReturn, ++level);
                            }
                        }
                    }
                    typeInfo.properties = modelInfoDic;
                    if (_type.IsEnum)
                    {
                        typeInfo.values = _type.GetEnumNames();
                    }
                    models.TryAdd(typeName, typeInfo);
                }
            }
        }

        private static bool IsRequired(PropertyInfo propertyInfo, XmlCommentDocumentationProvider docProvider)
        {
            if (propertyInfo.PropertyType.IsGenericType &&
                propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return false;

            return docProvider.IsRequired(propertyInfo);
        }

        private static string GetTypeName(Type type)
        {
            string name = type.Name;
            if (type.IsArray)
            {
                name = type.GetElementType().Name;
            }
            else if (type.IsGenericType)
            {
                name = type.GetGenericArguments().First().Name;

            }

            return name.ToLower().Replace("`1", "");
        }



        private static bool IsOutputable(Type type)
        {
            return !IgnoreTypes.Contains(type.Name.ToLower()) && (
                    (type.IsClass || type.IsInterface || type.IsEnum || type.IsArray) || (type.IsGenericType && !type.GetGenericArguments().First().IsPrimitive)
                );
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

        private static bool IsPropertyACollection(PropertyInfo property)
        {
            return property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }

        public static ApiSource[] GetApiSources(HttpControllerContext controllerContext)
        {
            var dir = ConfigurationManager.AppSettings["ApiSourceDir"] ?? Path.Combine("docs", "apiSources");
            var fullPath = Path.Combine(ServerPath, dir);
            string serverPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;
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
