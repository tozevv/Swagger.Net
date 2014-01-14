using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using Newtonsoft.Json;

namespace Swagger.Net
{
    public static class Helper
    {
        private static string _serverPath;
        public static readonly Regex RegexInteger = new Regex("int|double|float|long", RegexOptions.IgnoreCase);
        public static readonly Regex RegexString = new Regex("string|byte", RegexOptions.IgnoreCase);
        public static readonly Regex RegexDateTime = new Regex("dateTime|timeStamp", RegexOptions.IgnoreCase);
        public static readonly Regex RegexBoolean = new Regex("bool", RegexOptions.IgnoreCase);
        public static readonly Regex RegexArray = new Regex("ienumerable|isortablelist", RegexOptions.IgnoreCase);
        public static readonly Regex RegexRecursiveTypes = new Regex("task`1|nullable`1", RegexOptions.IgnoreCase);
        public static readonly Regex RegexArrayTypes = new Regex(@"\[\]|ienumerable|isorteablelist", RegexOptions.IgnoreCase);
        public static readonly Regex RegexMetadataTypes = new Regex("metadata`1|pagedmetadata`1|actionsmetadata`1", RegexOptions.IgnoreCase);

        private static readonly string[] IgnoreTypes = new[] { "void", "object", "string", "bool" };


        public static string ServerPath
        {
            get
            {
                if (_serverPath == null)
                {
                    _serverPath = HostingEnvironment.MapPath("~");
                }
                return _serverPath;
            }
        }


        public static SwaggerType GetSwaggerType(Type type)
        {
            var swaggerType = new SwaggerType();

            //Dig until finding a suitable type
            if (RegexRecursiveTypes.IsMatch(type.Name.ToLower()))
            {
                return GetSwaggerType(type.GetGenericArguments().First());
            }

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
                swaggerType.type = arrayType;
            }
            else
            {
                swaggerType.Name = GetTypeName(type);
                swaggerType.type = type;
            }

            return swaggerType;
        }

        public static void TryToAddModels(ConcurrentDictionary<string, TypeInfo> models, Type type, XmlCommentDocumentationProvider docProvider, ConcurrentDictionary<string, string> typesToReturn = null, int level = 0)
        {
            var _type = type;
            if (type.IsArray)
                _type = type.GetElementType();
            else if (type.IsGenericType && !RegexMetadataTypes.IsMatch(_type.Name.ToLower()))
                _type = type.GetGenericArguments().First();

            string typeName = GetTypeName(_type);

            if (models.All(m => m.Key != typeName))
            {
                if (IsOutputable(_type))
                {
                    var typeInfo = new TypeInfo { id = typeName };
                    if (!IgnoreTypes.Contains(_type.Name.ToLower()))
                    {
                        typeInfo.description = docProvider.GetSummary(_type);
                    }
                    //Ignore properties for .net types
                    if (!_type.Assembly.FullName.Contains("System") && !_type.Assembly.FullName.Contains("mscorlib"))
                    {
                        var modelInfoDic = new Dictionary<string, PropInfo>();
                        foreach (var propertyInfo in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (propertyInfo.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).FirstOrDefault() != null)
                                continue;

                            var propInfo = new PropInfo();

                            string propName = GetPropertyName(propertyInfo);
                            Type propType;
                            //If declaring type is Metadata, or derived,  get the generic type
                            if (RegexMetadataTypes.IsMatch(propertyInfo.ReflectedType.Name))
                            {
                                if (propertyInfo.Name == "Content")
                                    propType = propertyInfo.ReflectedType.GetGenericArguments().First();
                                else
                                {
                                    switch (propertyInfo.ReflectedType.Name.ToLower())
                                    {
                                        case "pagedmetadata`1":
                                            propType = Type.GetType("BSkyB.SuperMam.Web.Common.Messages.PagedMeta, SuperMam.Web.Common") ?? docProvider.GetType(propertyInfo);
                                            break;
                                        case "actionsmetadata`1":
                                            propType = Type.GetType("BSkyB.SuperMam.Web.Common.Messages.ActionsMeta, SuperMam.Web.Common") ?? docProvider.GetType(propertyInfo);
                                            break;
                                        default:
                                            propType = docProvider.GetType(propertyInfo);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                propType = docProvider.GetType(propertyInfo);
                            }
                            SwaggerType swaggerType = GetSwaggerType(propType);
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
                                    //propInfo.allowableValues = new AllowableValues()
                                    //    {
                                    //        valueType = GetTypeName(propertyInfo.PropertyType)
                                    //    };
                                    //propInfo.allowableValues.values = propertyInfo.PropertyType.GetEnumNames();
                                    modelInfoDic[propName].@enum = propertyInfo.PropertyType.GetEnumNames();
                                }
                                if (level < 10)
                                {
                                    TryToAddModels(models, swaggerType.type, docProvider, typesToReturn, ++level);
                                }
                            }
                        }
                        typeInfo.properties = modelInfoDic;
                    }
                    if (_type.IsEnum)
                    {
                        typeInfo.values = _type.GetEnumNames();
                    }

                    models.TryAdd(typeName, typeInfo);
                }
            }
        }

        private static string GetPropertyName(PropertyInfo propertyInfo)
        {
            var name = propertyInfo.Name;
            var jsonConvertAttribute = propertyInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault() as JsonPropertyAttribute;
            if (jsonConvertAttribute != null)
                name = jsonConvertAttribute.PropertyName;

            return name;
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
                if (RegexMetadataTypes.IsMatch(name))
                {
                    name = String.Format(@"{0}<{1}>", type.Name, GetTypeName(type.GetGenericArguments().First()));
                }
                else
                {
                    name = type.GetGenericArguments().First().Name;
                }
            }

            return name.ToLower().Replace("`1", "");
        }



        private static bool IsOutputable(Type type)
        {
            return !IgnoreTypes.Contains(type.Name.ToLower()) && (
                    (type.IsClass || type.IsInterface || type.IsEnum || type.IsArray) || (type.IsGenericType && !type.GetGenericArguments().First().IsPrimitive)
                );
        }
        
        public static ApiSource[] GetApiSources(HttpControllerContext controllerContext)
        {
            var apiAction = ConfigurationManager.AppSettings["SwaggerApiActionForSwaggerFiles"] ?? "";
            var dir = ConfigurationManager.AppSettings["SwaggerApiSourceDir"] ?? Path.Combine("docs", "apiSources");
            var fullPath = Path.Combine(ServerPath, dir);
            string serverPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;
            var dirs = Directory.GetDirectories(fullPath);
            if (!dirs.Any())
            {
                return null;
            }

            var sourcesList = dirs.Select(_dir =>
                {
                    var file = Directory.GetFiles(_dir, "base.json").FirstOrDefault();
                    if (file != null)
                    {
                        return new ApiSource(
                            Path.GetFileName(_dir),
                            String.Format("{0}?filePath={1}", apiAction, file.Replace(serverPhysicalPath, @""))
                            );
                    }
                    return null;
                });

            return sourcesList.Where(s => s != null).ToArray();
        }
    }
}
