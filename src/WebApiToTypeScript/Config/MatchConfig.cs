using System.Text.RegularExpressions;

namespace WebApiToTypeScript.Config
{
    public class MatchConfig
    {
        public string Match { get; set; }
        public string ExcludeMatch { get; set; }

        protected Regex MatchRegex { get; set; } = null;
        protected Regex ExcludeMatchRegex { get; set; } = null;

        public static bool IsMatch(MatchConfig matchConfig, string typeFullName)
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

            var isMatch = matchConfig.MatchRegex != null && matchConfig.MatchRegex.IsMatch(typeFullName)
                    && (!excludeMatchConfigExists || !matchConfig.ExcludeMatchRegex.IsMatch(typeFullName));

            return isMatch;
        }
    }
}