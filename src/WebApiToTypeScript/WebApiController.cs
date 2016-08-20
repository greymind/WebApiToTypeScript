using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace WebApiToTypeScript
{
    public class WebApiController
    {
        private readonly Regex RouteParameterRegex
            = new Regex("{(.*)}");

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

            BaseRoute = GetEndpointRoute(apiController) ?? Name;

            UpdateRouteParts();
            UpdateBaseEndpoint();

            UpdateActions(apiController);
        }

        private void UpdateActions(TypeDefinition apiController)
        {
            var methodNames = new HashSet<string>();

            Actions = apiController.Methods
                .Where(m => m.IsPublic
                    && m.HasCustomAttributes
                    && m.CustomAttributes.Any(a => WebApiAction.HttpVerbs.Contains(a.AttributeType.Name)))
                .Select(m => new WebApiAction
                (
                    baseRouteParts: RouteParts,
                    method: m,
                    name: GetUniqueMethodName(methodNames, m.Name),
                    verb: m.CustomAttributes.Single(a => WebApiAction.HttpVerbs.Contains(a.AttributeType.Name))
                        .AttributeType.Name
                ))
                .ToList();
        }

        private void UpdateBaseEndpoint()
        {
            var baseEndpointParts = RouteParts
                .Select(routePart => string.IsNullOrEmpty(routePart.ParameterName)
                    ? routePart.Name
                    : $"${{this.{routePart.ParameterName}}}");

            BaseEndpoint = $"/{string.Join("/", baseEndpointParts)}";
        }

        private void UpdateRouteParts()
        {
            RouteParts = BaseRoute.Split('/')
                .Select(p => new WebApiRoutePart
                {
                    Name = p,
                    ParameterName = RouteParameterRegex.Match(p).Groups[1].Value
                })
                .ToList();

            foreach (var routePart in RouteParts)
            {
                if (!string.IsNullOrEmpty(routePart.ParameterName))
                    routePart.ParameterName = routePart.ParameterName.Split(':').First();
            }
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