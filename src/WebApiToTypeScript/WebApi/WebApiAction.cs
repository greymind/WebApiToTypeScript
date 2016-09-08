using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript.WebApi
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

        public List<WebApiRoutePart> QueryStringParameters { get; }
            = new List<WebApiRoutePart>();

        public List<WebApiRoutePart> BodyParameters { get; }
            = new List<WebApiRoutePart>();

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

            GetQueryStringAndBodyRouteParts(baseRouteParts);
        }

        private void GetQueryStringAndBodyRouteParts(List<WebApiRoutePart> baseRouteParts)
        {
            var actionParameters = Method.Parameters
                .Where(p => !baseRouteParts.Any(brp => brp.ParameterName == p.Name)
                    && !RouteParts.Any(rp => rp.ParameterName == p.Name))
                .ToList();

            var isBodyAllowed = Verbs.Contains(WebApiHttpVerb.Post)
                || Verbs.Contains(WebApiHttpVerb.Put);

            var fromBodyAttributeName = "FromBodyAttribute";
            var fromUriAttributeName = "FromUriAttribute";

            var isThereAnythingFromBody = actionParameters
                .Any(ap => Helpers.HasCustomAttribute(ap, fromBodyAttributeName));

            foreach (var actionParameter in actionParameters)
            {
                var isFromBody = Helpers.HasCustomAttribute(actionParameter, fromBodyAttributeName);
                var isFromUri = Helpers.HasCustomAttribute(actionParameter, fromUriAttributeName);
                var isPrimitive = actionParameter.ParameterType.IsPrimitive;

                if (!isPrimitive)
                {
                    var typeService = new TypeService();
                    var nullableType = typeService.StripNullable(actionParameter.ParameterType);
                    var isNullable = nullableType != null;
                    isPrimitive = isNullable && typeService.GetPrimitiveTypeScriptType(nullableType) != null;
                }

                if (isBodyAllowed 
                    && ((isThereAnythingFromBody && isFromBody)
                        || (!isThereAnythingFromBody && !isFromUri && !isPrimitive)))
                {
                    BodyParameters.Add(new WebApiRoutePart
                    {
                        Name = actionParameter.Name,
                        ParameterName = actionParameter.Name,
                        Parameter = actionParameter,
                        CustomAttributes = new List<string> { fromBodyAttributeName }
                    });
                }
                else
                {
                    var queryStringRoutePart = new WebApiRoutePart
                    {
                        Name = actionParameter.Name,
                        ParameterName = actionParameter.Name,
                        Parameter = actionParameter
                    };

                    if (actionParameter.HasCustomAttributes)
                    {
                        queryStringRoutePart.CustomAttributes = actionParameter.CustomAttributes
                            .Select(a => a.AttributeType.Name)
                            .ToList();
                    }

                    QueryStringParameters.Add(queryStringRoutePart);
                }
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