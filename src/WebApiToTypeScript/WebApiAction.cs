using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace WebApiToTypeScript
{
    public class WebApiAction
    {
        public static readonly string[] HttpVerbs = new string[] {
            "HttpGetAttribute",
            "HttpPostAttribute",
            "HttpPutAttribute",
            "HttpDeleteAttribute"
        };

        public string Name { get; set; }
        public string Route { get; set; }

        public MethodDefinition Method { get; set; }
        public string Verb { get; set; }

        public List<PropertyDefinition> RouteProperties { get; set; }
            = new List<PropertyDefinition>();

        public List<ParameterDefinition> QueryStringParameters { get; set; }
            = new List<ParameterDefinition>();

        public WebApiAction(List<WebApiRoutePart> baseRouteParts, MethodDefinition method, string name,
            string verb)
        {
            Method = method;
            Name = name;
            Verb = verb;

            Route = GetMethodRoute(Method) ?? Name;

            QueryStringParameters = Method.Parameters
                .Where(p => !baseRouteParts.Any(brp => brp.ParameterName == p.Name)
                    && !(p.HasCustomAttributes
                        && p.CustomAttributes.Any(a => a.AttributeType.Name == "FromBodyAttribute")))
                .ToList();
        }

        private string GetMethodRoute(MethodDefinition method)
        {
            return method.CustomAttributes
                ?.SingleOrDefault(a => a.AttributeType.Name == "RouteAttribute")
                ?.ConstructorArguments
                .First()
                .Value
                .ToString();
        }
    }
}