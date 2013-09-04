using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.XPath;

namespace Swagger.Net
{
    /// <summary>
    /// Accesses the XML doc blocks written in code to further document the API.
    /// All credit goes to: <see cref="http://blogs.msdn.com/b/yaohuang1/archive/2012/05/21/asp-net-web-api-generating-a-web-api-help-page-using-apiexplorer.aspx"/>
    /// </summary>
    public class XmlCommentDocumentationProvider : IDocumentationProvider
    {
        private const string _methodExpression = "/doc/members/member[@name='M:{0}']";
        private const string _propertyExpression = "/doc/members/member[@name='P:{0}']";
        private const string _typeExpression = "/doc/members/member[@name='T:{0}']";
        private static Regex nullableTypeNameRegex = new Regex(@"(.*\.Nullable)" + Regex.Escape("`1[[") + "([^,]*),.*");

        private static IDictionary<string, XPathNavigator> _documentNavigators = new Dictionary<string, XPathNavigator>();

        public XmlCommentDocumentationProvider(IEnumerable<string> documentPaths)
        {
            foreach (var documentPath in documentPaths)
            {
                try
                {
                    XPathDocument xpath = new XPathDocument(documentPath);
                    _documentNavigators.Add(Path.GetFileNameWithoutExtension(documentPath), xpath.CreateNavigator());
                }
                catch (Exception) { }
            }

        }

        private static XPathNavigator GetNavigator(string assemblyName)
        {
            return _documentNavigators.Keys.Contains(assemblyName) ? _documentNavigators[assemblyName] : null;
        }

        public virtual string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                XPathNavigator memberNode = GetMemberNode(reflectedParameterDescriptor.ActionDescriptor);
                if (memberNode != null)
                {
                    string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                    XPathNavigator parameterNode = memberNode.SelectSingleNode(string.Format("param[@name='{0}']", parameterName));
                    if (parameterNode != null)
                    {
                        return parameterNode.Value.Trim();
                    }
                }
            }

            return "No Documentation Found.";
        }

        public virtual bool GetRequired(HttpParameterDescriptor parameterDescriptor)
        {
            ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                return !reflectedParameterDescriptor.ParameterInfo.IsOptional;
            }

            return true;
        }

        public virtual string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator summaryNode = memberNode.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    return summaryNode.Value.Trim();
                }
            }

            return "No Documentation Found.";
        }

        public virtual string GetNotes(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator summaryNode = memberNode.SelectSingleNode("remarks");
                if (summaryNode != null)
                {
                    return summaryNode.Value.Trim();
                }
            }

            return "No Documentation Found.";
        }

        public virtual ResponseMeta GetResponseType(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                Type returnType = reflectedActionDescriptor.MethodInfo.ReturnType;
                var membernode = GetMemberNode(actionDescriptor);
                if (membernode != null)
                {
                    var overrideReturn = membernode.SelectSingleNode("overrideReturn");
                    if (overrideReturn != null)
                    {
                        var type = overrideReturn.GetAttribute("type", String.Empty);
                        returnType = Type.GetType(type, true, false);
                    }
                }

                if (returnType.IsGenericType)
                {
                    StringBuilder sb =
                        new StringBuilder(reflectedActionDescriptor.MethodInfo.ReturnParameter.ParameterType.Name);
                    sb.Append("<");
                    Type[] types =
                        reflectedActionDescriptor.MethodInfo.ReturnParameter.ParameterType.GetGenericArguments();
                    for (int i = 0; i < types.Length; i++)
                    {
                        sb.Append(types[i].Name);
                        if (i != (types.Length - 1)) sb.Append(", ");
                    }
                    sb.Append(">");
                    return new ResponseMeta()
                        {
                            Name = sb.Replace("`1", "").ToString(),
                            Type = returnType

                        };
                }
                else
                {
                    return new ResponseMeta()
                        {
                            Name = returnType.Name,
                            Type = returnType
                        };

                }
            }

            return null;
        }

        public virtual string GetNickname(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                return reflectedActionDescriptor.MethodInfo.Name;
            }

            return "NicknameNotFound";
        }

        public string GetSummary(PropertyInfo propertyInfo)
        {
            if (propertyInfo.DeclaringType == null)
                return null;

            string propertyFullname = String.Format("{0}.{1}", propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            string selectExpression = string.Format(_propertyExpression, propertyFullname);
            return GetSummary(selectExpression, propertyInfo.DeclaringType.Assembly.GetName().Name);
        }

        public string GetSummary(Type type)
        {
            string typeName = type.FullName;
            string selectExpression = string.Format(_typeExpression, typeName);
            return GetSummary(selectExpression, type.Assembly.GetName().Name);
        }

        private string GetSummary(string selectExpression, string assemblyName)
        {
            var navigator = GetNavigator(assemblyName);
            if (navigator == null)
                return null;

            XPathNavigator node = navigator.SelectSingleNode(selectExpression);
            if (node == null)
                return null;

            var summary = node.SelectSingleNode("summary");
            if (summary == null)
                return null;

            return summary.Value;
        }

        private XPathNavigator GetMemberNode(HttpActionDescriptor actionDescriptor)
        {
            ReflectedHttpActionDescriptor reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                var navigator = GetNavigator(reflectedActionDescriptor.MethodInfo.DeclaringType.Assembly.GetName().Name);
                if (navigator == null)
                    return null;

                string selectExpression = string.Format(_methodExpression, GetMemberName(reflectedActionDescriptor.MethodInfo));
                XPathNavigator node = navigator.SelectSingleNode(selectExpression);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        private static string GetMemberName(MethodInfo method)
        {
            string name = string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name);
            var parameters = method.GetParameters();
            if (parameters.Length != 0)
            {
                string[] parameterTypeNames = parameters.Select(param => ProcessTypeName(param.ParameterType.FullName)).ToArray();
                name += string.Format("({0})", string.Join(",", parameterTypeNames));
            }

            return name;
        }

        private static string ProcessTypeName(string typeName)
        {
            //handle nullable
            var result = nullableTypeNameRegex.Match(typeName);
            if (result.Success)
            {
                return string.Format("{0}{{{1}}}", result.Groups[1].Value, result.Groups[2].Value);
            }
            return typeName;
        }

        public List<ResponseMessage> GetResponseCodes(HttpActionDescriptor actionDescriptor)
        {
            XPathNavigator memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                XPathNavigator responsesNode = memberNode.SelectSingleNode("responseCodes");
                if (responsesNode != null)
                {
                    var responses = responsesNode.SelectChildren("response", string.Empty);
                    if (responses.Count > 0)
                    {
                        var responseList = new List<ResponseMessage>();
                        while (responses.MoveNext())
                        {
                            var response = responses.Current;
                            int code;
                            Int32.TryParse(response.SelectSingleNode("code").Value, out code);
                            var responseMessage = new ResponseMessage()
                                {
                                    code = code,
                                    message = response.SelectSingleNode("message").Value
                                };

                            responseList.Add(responseMessage);
                        }
                        return responseList;
                    }
                }

            }
            return null;
        }

        public string[] GetPossibleValues(HttpParameterDescriptor parameterDescriptor)
        {

            if (!parameterDescriptor.ParameterType.IsEnum)
            {
                return null;
            }

            return parameterDescriptor.ParameterType.GetEnumNames();

        }

        public string GetDefaultParameterValue(HttpParameterDescriptor parameterDescriptor)
        {
            ReflectedHttpParameterDescriptor reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                XPathNavigator memberNode = GetMemberNode(reflectedParameterDescriptor.ActionDescriptor);
                if (memberNode != null)
                {
                    string parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                    XPathNavigator parameterNode = memberNode.SelectSingleNode(string.Format("param[@name='{0}']", parameterName));
                    if (parameterNode != null)
                    {
                        return parameterNode.GetAttribute("default", String.Empty);
                    }
                }
            }
            return null;
        }

        public bool IsRequired(PropertyInfo propertyInfo)
        {
            bool required = false;
            var navigator = GetNavigator(propertyInfo.PropertyType.Assembly.GetName().Name);
            if (navigator == null)
                return required;

            string propertyFullname = String.Format("{0}.{1}", propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            string selectExpression = string.Format(_propertyExpression, propertyFullname);
            var node = navigator.SelectSingleNode(selectExpression);
            if (node == null)
                return required;

            node = node.SelectSingleNode("notRequired");
            if (node == null)
                return required;

            Boolean.TryParse(node.Value, out required);
            return required;
        }
    }

    public class ResponseMeta
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
}
