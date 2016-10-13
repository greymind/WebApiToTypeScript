using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript
{
    public static class Helpers
    {
        private static readonly Regex RouteParameterRegex
            = new Regex("{(.*)}");

        public static string ToCamelCaseFromPascalCase(string inPascalCase)
        {
            if (!string.IsNullOrEmpty(inPascalCase))
                return $"{char.ToLower(inPascalCase.First())}{inPascalCase.Substring(1)}";
            else
                return inPascalCase;
        }

        public static string ToPascalCaseFromCamelCase(string inCamelCase)
        {
            if (!string.IsNullOrEmpty(inCamelCase))
                return $"{char.ToUpper(inCamelCase.First())}{inCamelCase.Substring(1)}";
            else
                return inCamelCase;
        }

        public static string ToPascalCaseFromKebabCase(string inKebabCase)
        {
            if (!string.IsNullOrEmpty(inKebabCase))
            {
                var parts = inKebabCase
                    .Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => $"{char.ToUpper(v.First())}{v.Substring(1)}");

                return string.Join(string.Empty, parts);
            }
            else
                return inKebabCase;
        }

        public static string GetBaseEndpoint(List<WebApiRoutePart> routeParts)
        {
            var baseEndpointParts = routeParts
                .Select(routePart => string.IsNullOrEmpty(routePart.ParameterName)
                    ? routePart.Name
                    : $"${{this.{routePart.ParameterName}}}");

            return baseEndpointParts.Any()
                ? $"/{string.Join("/", baseEndpointParts)}"
                : string.Empty;
        }

        public static List<WebApiRoutePart> GetRouteParts(string baseRoute)
        {
            var routeParts = baseRoute.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => new WebApiRoutePart
                {
                    Name = p,
                    ParameterName = RouteParameterRegex.Match(p).Groups[1].Value,
                    IsOptional = false
                })
                .ToList();

            foreach (var routePart in routeParts)
            {
                if (!string.IsNullOrEmpty(routePart.ParameterName))
                {
                    var routeConstraints = routePart.ParameterName.Split(':');
                    routePart.ParameterName = routeConstraints.First();
                    routePart.Constraints = routeConstraints.Skip(1)
                        .ToList();
                }
            }

            return routeParts;
        }

        public static bool HasCustomAttribute(ParameterDefinition parameter, string attributeName)
        {
            return parameter.HasCustomAttributes
                && parameter.CustomAttributes.Any(a => a.AttributeType.Name == attributeName);
        }
    }
}