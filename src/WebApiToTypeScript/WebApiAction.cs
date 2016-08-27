using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace WebApiToTypeScript
{
    public class WebApiAction
    {
        public string Name { get; set; }
        public string Route { get; set; }
        public string Endpoint { get; set; }

        public MethodDefinition Method { get; set; }

        public List<WebApiHttpVerb> Verbs { get; set; }
            = new List<WebApiHttpVerb>();

        public List<WebApiRoutePart> RouteParts { get; set; }
            = new List<WebApiRoutePart>();

        public List<ParameterDefinition> QueryStringParameters { get;  }
            = new List<ParameterDefinition>();

        public List<ParameterDefinition> BodyParameters { get;  }
            = new List<ParameterDefinition>();

        public WebApiAction(List<WebApiRoutePart> baseRouteParts, MethodDefinition method, string name)
        {
            Method = method;
            Name = name;

            Verbs = Method.CustomAttributes
                .Select(a => WebApiHttpVerb.Verbs.SingleOrDefault(v => v.VerbAttribute == a.AttributeType.Name))
                .Where(a => a != null)
                .ToList();

            Route = GetMethodRoute(Method) ?? string.Empty;

            RouteParts = Helpers.GetRouteParts(Route);
            Endpoint = Helpers.GetBaseEndpoint(RouteParts);

            var actionParameters = Method.Parameters
                .Where(p => !baseRouteParts.Any(brp => brp.ParameterName == p.Name)
                    && !RouteParts.Any(rp => rp.ParameterName == p.Name));

            var isBodyAllowed = Verbs.Contains(WebApiHttpVerb.Post)
                || Verbs.Contains(WebApiHttpVerb.Put);

            foreach (var actionParameter in actionParameters)
            {
                var isFromBody = actionParameter.HasCustomAttributes
                    && actionParameter.CustomAttributes.Any(a => a.AttributeType.Name == "FromBodyAttribute");

                if (isBodyAllowed && isFromBody)
                    BodyParameters.Add(actionParameter);
                else
                    QueryStringParameters.Add(actionParameter);
            }
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