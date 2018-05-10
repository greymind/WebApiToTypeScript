using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript.WebApi
{
    public class WebApiAction : ServiceAware
    {
        public string Name { get; set; }
        public string Route { get; set; }
        public string Endpoint { get; set; }

        public WebApiController Controller { get; set; }

        public MethodDefinition Method { get; set; }

        public List<WebApiHttpVerb> Verbs { get; set; }
            = new List<WebApiHttpVerb>();

        public List<WebApiRoutePart> RouteParts { get; set; }
            = new List<WebApiRoutePart>();

        public List<WebApiRoutePart> QueryStringParameters { get; }
            = new List<WebApiRoutePart>();

        public List<WebApiRoutePart> BodyParameters { get; }
            = new List<WebApiRoutePart>();

        public WebApiAction(WebApiController controller, MethodDefinition method, string name)
        {
            Controller = controller;

            Method = method;
            Name = name;

            Verbs = Method.CustomAttributes
                .Select(a => WebApiHttpVerb.Verbs.SingleOrDefault(v => v.VerbAttribute == a.AttributeType.Name))
                .Where(a => a != null)
                .ToList();
            if (Verbs.Count == 0)
                Verbs.Add(WebApiHttpVerb.Get);

            Route = GetMethodRoute(Method) ?? string.Empty;

            RouteParts = Helpers.GetRouteParts(Route);
            Endpoint = Helpers.GetBaseEndpoint(RouteParts);

            BuildQueryStringAndBodyRouteParts();
        }

        public void GetReturnTypes(out string typeScriptReturnType, out string typeScriptTypeForCall)
        {
            if (Config.GenerateEndpointsReturnTypes)
            {
                var returnTypeScriptType = TypeService.GetTypeScriptType(Method.ReturnType, "");

                var collectionString = Helpers.GetCollectionPostfix(returnTypeScriptType.CollectionLevel);

                var typeName = returnTypeScriptType.InterfaceName;

                if (typeName != "any")
                {
                    typeScriptReturnType = $"<{typeName}{collectionString}>";
                    typeScriptTypeForCall = "";

                    return;
                }
            }

            typeScriptReturnType = "<TView>";
            typeScriptTypeForCall = "<TView>";
        }

        public string GetActionNameForVerb(WebApiHttpVerb verb)
        {
            var verbPostfix = Verbs.Count > 1
                ? verb == WebApiHttpVerb.Post ? "New" : "Existing"
                : string.Empty;

            return $"{Name}{verbPostfix}";
        }

        public string GetCallArgumentValue(WebApiHttpVerb verb)
        {
            var isFormBody = verb == WebApiHttpVerb.Post || verb == WebApiHttpVerb.Put;

            var callArgumentValueString = BodyParameters
                .Select(routePart =>
                {
                    var typeScriptType = routePart.GetTypeScriptType()
                        .TypeName;

                    var valueFormat = $"{routePart.Name}";

                    switch (typeScriptType)
                    {
                        case "string":
                            valueFormat = $"`\"${{{routePart.Name}}}\"`";
                            break;
                    }

                    return $"{routePart.Name} != null ? {valueFormat} : null";
                })
                .SingleOrDefault();

            if(Config.ServiceUseAngularNext)
            {
                return (!isFormBody || string.IsNullOrEmpty(callArgumentValueString))
                 ? "null"
                 : $"{callArgumentValueString}";
            }
            else
            {
                return (!isFormBody || string.IsNullOrEmpty(callArgumentValueString))
                 ? "null, httpConfig"
                 : $"{callArgumentValueString}, httpConfig";
            }            
        }

        public string GetCallArgumentDefinition(WebApiHttpVerb verb)
        {
            var isFormBody = verb == WebApiHttpVerb.Post || verb == WebApiHttpVerb.Put;

            if (!isFormBody)
                return Config.ServiceUseAngularNext ? "" : "httpConfig?: ng.IRequestShortcutConfig";

            var bodyParam = BodyParameters
                .Select(a => a.GetParameterString(withOptionals: false, interfaceName: true))
                .SingleOrDefault();

            if (Config.ServiceUseAngularNext)
            {
                return string.IsNullOrWhiteSpace(bodyParam)
                ? ""
                : $"{bodyParam}";
            }
            else
            {
                return string.IsNullOrWhiteSpace(bodyParam)
                ? "httpConfig?: ng.IRequestShortcutConfig"
                : $"{bodyParam}, httpConfig?: ng.IRequestShortcutConfig";
            }
        }

        public string GetConstructorParameterNamesList()
        {
            var constructorParameterNames = GetConstructorParameterMappings()
                .Select(p => p.Name);

            return string.Join(", ", constructorParameterNames);
        }

        public string GetConstructorParametersList()
        {
            var constructorParameterMappings = GetConstructorParameterMappings();

            var constructorParameterStrings = constructorParameterMappings
                .Select(p => p.StringWithOptionals);

            var constructorParametersList =
                string.Join(", ", constructorParameterStrings);

            return constructorParametersList;
        }

        public ConstructorParameterMapping[] GetConstructorParameterMappings()
        {
            var tempConstructorParameters = Method.Parameters
                .Select(p => new
                {
                    Parameter = p,
                    RoutePart = Controller.RouteParts.SingleOrDefault(brp => brp.ParameterName == p.Name)
                        ?? RouteParts.SingleOrDefault(rp => rp.ParameterName == p.Name)
                        ?? QueryStringParameters.SingleOrDefault(qsp => qsp.Name == p.Name)
                })
                .Where(cp => cp.RoutePart != null)
                .ToList();

            var constructorParameters = new List<WebApiRoutePart>();
            foreach (var tcp in tempConstructorParameters)
            {
                if (tcp.RoutePart.Parameter == null)
                    tcp.RoutePart.Parameter = tcp.Parameter;

                constructorParameters.Add(tcp.RoutePart);
            }

            var constructorParameterMappings = constructorParameters
                .Select(routePart => new ConstructorParameterMapping
                {
                    IsOptional = routePart.IsOptional && TypeService.IsParameterOptional(routePart.Parameter),
                    TypeMapping = routePart.GetTypeMapping(),
                    Name = routePart.Parameter.Name,
                    StringWithOptionals = routePart.GetParameterString(interfaceName: true),
                    String = routePart.GetParameterString(withOptionals: false, interfaceName: true)
                })
                .OrderBy(p => p.IsOptional)
                .ToArray();

            return constructorParameterMappings;
        }

        private void BuildQueryStringAndBodyRouteParts()
        {
            var actionParameters = Method.Parameters
                .Where(p => Controller.RouteParts.All(brp => brp.ParameterName != p.Name)
                    && RouteParts.All(rp => rp.ParameterName != p.Name))
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
                    var strippedType = TypeService.StripGenerics(actionParameter.ParameterType, actionParameter.Name, out bool isNullable, out int collectionLevel);
                    isPrimitive = isNullable && TypeService.GetPrimitiveTypeScriptType(strippedType.FullName) != null;
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
                        CustomAttributes = new List<string> { fromBodyAttributeName },
                        IsOptional = false
                    });
                }
                else
                {
                    var queryStringRoutePart = new WebApiRoutePart
                    {
                        Name = actionParameter.Name,
                        ParameterName = actionParameter.Name,
                        Parameter = actionParameter,
                        IsOptional = true
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