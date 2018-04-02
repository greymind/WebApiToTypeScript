using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace WebApiToTypeScript.WebApi
{
    public class WebApiController
    {
        public string Name { get; set; }
        public string BaseRoute { get; set; }
        public string BaseEndpoint { get; set; }

        public List<WebApiAction> Actions { get; set; }
            = new List<WebApiAction>();

        public List<WebApiRoutePart> RouteParts { get; set; }
            = new List<WebApiRoutePart>();

        public WebApiController(TypeDefinition apiController, Config.Config config = null)
        {
            Name = apiController.Name
                .Replace("Controller", string.Empty);

            BaseRoute = GetEndpointRoute(apiController, config) ?? $"api/{Name}";

            RouteParts = Helpers.GetRouteParts(BaseRoute);
            BaseEndpoint = Helpers.GetBaseEndpoint(RouteParts);

            BuildActions(apiController);
        }

        private void BuildActions(TypeDefinition apiController)
        {
            var methodNames = new HashSet<string>();

            Actions = apiController.Methods
                .Where(m => m.IsPublic
                    && m.HasCustomAttributes
                    && m.CustomAttributes.Any(a => WebApiHttpVerb.Verbs.Any(v => v.VerbAttribute == a.AttributeType.Name)
                        || a.AttributeType.Name == "RouteAttribute"))
                .Select(m => new WebApiAction
                (
                    controller: this,
                    method: m,
                    name: GetUniqueMethodName(methodNames, m.Name)
                ))
                .ToList();
        }

        private string GetEndpointRoute(TypeDefinition apiController, Config.Config config = null)
        {
            //var e = apiController.CustomAttributes.FirstOrDefault(p => p.AttributeType.Name == "InsightRouteAttribute")?.AttributeType.GetType();
            //var x = e != null ? System.Activator.CreateInstance(e) : null;

            // Temporary hacks
            if (config.ServiceUseAngularNext)
            {
                var y = apiController.CustomAttributes
                    ?.SingleOrDefault(a => a.AttributeType.Name == "InsightRouteAttribute");

                if (y != null)
                {
                    var c = y.ConstructorArguments?.FirstOrDefault();
                    var d = (c == null || !c.HasValue || c.Value.Value == null) ? "" : c.Value.Value.ToString();

                    return "insight/api/" + d;
                }

                return apiController.CustomAttributes
                    ?.SingleOrDefault(a => a.AttributeType.Name == "RoutePrefixAttribute")
                    ?.ConstructorArguments
                    .First()
                    .Value
                    .ToString();
            }

            return apiController.CustomAttributes
                ?.SingleOrDefault(a => a.AttributeType.Name == "RoutePrefixAttribute")
                ?.ConstructorArguments
                .First()
                .Value
                .ToString();
        }

        private string GetUniqueMethodName(HashSet<string> methodNames, string originalMethodName)
        {
            var methodName = originalMethodName;

            var counter = 1;
            while (methodNames.Contains(methodName))
                methodName = $"{originalMethodName}{counter++}";

            methodNames.Add(methodName);

            return methodName;
        }
    }
}