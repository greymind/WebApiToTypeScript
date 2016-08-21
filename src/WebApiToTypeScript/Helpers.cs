using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApiToTypeScript
{
    public static class Helpers
    {
        private static readonly Regex RouteParameterRegex
            = new Regex("{(.*)}");

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
                    ParameterName = RouteParameterRegex.Match(p).Groups[1].Value
                })
                .ToList();

            foreach (var routePart in routeParts)
            {
                if (!string.IsNullOrEmpty(routePart.ParameterName))
                    routePart.ParameterName = routePart.ParameterName.Split(':').First();
            }

            return routeParts;
        }
    }
}