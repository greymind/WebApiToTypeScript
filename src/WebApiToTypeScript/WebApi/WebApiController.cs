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

        public WebApiController(Config.Config config, TypeDefinition apiController)
        {
            Name = apiController.Name
                .Replace("Controller", string.Empty);

            BaseRoute = GetEndpointRoute(apiController) ?? $"{Name}";

            RouteParts = Helpers.GetRouteParts(BaseRoute);
            BaseEndpoint = Helpers.GetBaseEndpoint(config.RoutePrefix, RouteParts);

            BuildActions(apiController);
        }

        private void BuildActions(TypeDefinition apiController)
        {
            var methodNames = new HashSet<string>();

            Actions = apiController.Methods
                .Where(m => m.IsPublic
                    && m.HasCustomAttributes
                    && (m.CustomAttributes.Any(a => WebApiHttpVerb.Verbs.Any(v => v.VerbAttribute == a.AttributeType.Name))
                        || WebApiHttpVerb.Verbs.Any(v => v.Verb == m.Name)))
                .Select(m => new WebApiAction
                (
                    controller: this,
                    method: m,
                    name: GetUniqueMethodName(methodNames, m)
                ))
                .ToList();
        }

        private string GetEndpointRoute(TypeDefinition apiController)
        {
            return apiController.CustomAttributes
                ?.SingleOrDefault(a => a.AttributeType.Name == "RoutePrefixAttribute")
                ?.ConstructorArguments
                .First()
                .Value
                .ToString();
        }

        private string GetUniqueMethodName(HashSet<string> methodNames, MethodDefinition method)
        {
            var resolvedMethodName = method.Name;

            // Try to handle Get and GetAll uniquely
            if (resolvedMethodName == WebApiHttpVerb.Get.Verb)
            {
                var isGetAll = method.Parameters.Count == 0
                    || method.Parameters.All(p => Helpers.HasCustomAttribute(p, WebApiAction.FromUriAttributeName));

                resolvedMethodName = isGetAll
                    ? $"{WebApiHttpVerb.Get.Verb}All"
                    : $"{WebApiHttpVerb.Get.Verb}";
            }

            var methodName = resolvedMethodName;

            // If conflict still exists, just append a counter to differentiate the overloads

            var counter = 1;
            while (methodNames.Contains(methodName))
                methodName = $"{resolvedMethodName}{counter++}";

            methodNames.Add(methodName);

            return methodName;
        }
    }
}