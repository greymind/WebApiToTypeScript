using Mono.Cecil;
using System.Linq;
using System.Text.RegularExpressions;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript.Config
{
    public class MatchConfigWithBaseType : MatchConfig
    {
        public string BaseTypeName { get; set; }

        public static bool IsMatch(TypeService typeService, MatchConfigWithBaseType matchConfig, TypeDefinition type)
        {
            var matchConfigExists = !string.IsNullOrEmpty(matchConfig.Match);

            if (!matchConfigExists)
                return false;

            if (matchConfigExists && matchConfig.MatchRegex == null)
            {
                matchConfig.MatchRegex = new Regex(matchConfig.Match);
            }

            var excludeMatchConfigExists = !string.IsNullOrEmpty(matchConfig.ExcludeMatch);
            if (excludeMatchConfigExists && matchConfig.ExcludeMatchRegex == null)
            {
                matchConfig.ExcludeMatchRegex = new Regex(matchConfig.ExcludeMatch);
            }

            var baseTypeNameConfigExists = !string.IsNullOrEmpty(matchConfig.BaseTypeName);

            var isMatch = matchConfig.MatchRegex != null && matchConfig.MatchRegex.IsMatch(type.FullName)
                    && (!excludeMatchConfigExists || !matchConfig.ExcludeMatchRegex.IsMatch(type.FullName));

            var doesBaseTypeMatch = !baseTypeNameConfigExists
                    || typeService.GetBaseTypes(type).Any(t => t.FullName.EndsWith(matchConfig.BaseTypeName) || t.FullName.StartsWith(matchConfig.BaseTypeName));

            return isMatch && doesBaseTypeMatch;
        }
    }
}