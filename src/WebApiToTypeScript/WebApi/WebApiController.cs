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

        public WebApiController(TypeDefinition apiController)
        {
            Name = apiController.Name
                .Replace("Controller", string.Empty);

            BaseRoute = GetEndpointRoute(apiController) ?? $"api/{Name}";

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
                    && m.CustomAttributes.Any(a => WebApiHttpVerb.Verbs.Any(v => v.VerbAttribute == a.AttributeType.Name)))
                .Select(m => new WebApiAction
                (
                    controller: this,
                    method: m,
                    name: GetUniqueMethodName(methodNames, m.Name)
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