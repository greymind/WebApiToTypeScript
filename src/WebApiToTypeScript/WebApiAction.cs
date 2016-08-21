using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace WebApiToTypeScript
{
    public class WebApiAction
    {
        public string Name { get; set; }
        public string Route { get; set; }
        public string Endpoint { get; set; }

        public MethodDefinition Method { get; set; }
        public string Verb { get; set; }

        public List<WebApiRoutePart> RouteParts { get; set; }
            = new List<WebApiRoutePart>();

        public List<ParameterDefinition> QueryStringParameters { get; set; }
            = new List<ParameterDefinition>();

        public WebApiAction(List<WebApiRoutePart> baseRouteParts, MethodDefinition method, string name,
            string verb)
        {
            Method = method;
            Name = name;
            Verb = verb;

            Route = GetMethodRoute(Method) ?? string.Empty;

            RouteParts = Helpers.GetRouteParts(Route);
            Endpoint = Helpers.GetBaseEndpoint(RouteParts);

            QueryStringParameters = Method.Parameters
                .Where(p => !baseRouteParts.Any(brp => brp.ParameterName == p.Name)
                    && !RouteParts.Any(rp => rp.ParameterName == p.Name)
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