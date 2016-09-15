using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using WebApiToTypeScript.Config;
using WebApiToTypeScript.Types;

namespace WebApiToTypeScript.WebApi
{
    public class WebApiRoutePart : ServiceAware
    {
        public string Name { get; set; }
        public string ParameterName { get; set; }
        public ParameterDefinition Parameter { get; set; }

        public bool IsOptional { get; set; }
            = true;

        public List<string> Constraints { get; set; }
            = new List<string>();

        public List<string> CustomAttributes { get; set; }
            = new List<string>();

        public TypeScriptType GetTypeScriptType()
        {
            return TypeService.GetTypeScriptType(Parameter.ParameterType, ParameterName, GetTypeMapping);
        }

        public string GetParameterString(bool withOptionals = true, bool interfaceName = false)
        {
            var isOptional = withOptionals && IsOptional && TypeService.IsParameterOptional(Parameter);
            var typeScriptType = GetTypeScriptType();

            var collectionString = typeScriptType.IsCollection ? "[]" : string.Empty;

            var typeName = interfaceName
                ? typeScriptType.InterfaceName
                : typeScriptType.TypeName;

            return $"{Parameter.Name}{(isOptional ? "?" : "")}: {typeName}{collectionString}";
        }

        public TypeMapping GetTypeMapping()
        {
            return GetTypeMapping(ParameterName, Parameter.ParameterType.FullName);
        }

        private TypeMapping GetTypeMapping(string parameterName, string typeFullName)
        {
            if (Parameter == null)
                return null;

            var typeMapping = Config.TypeMappings
                .FirstOrDefault(MatchTypeMapping);

            return typeMapping;
        }

        private bool MatchTypeMapping(TypeMapping typeMapping)
        {
            var parameterName = Parameter.Name;
            var typeFullName = Parameter.ParameterType.FullName;

            var doesTypeNameMatch = typeFullName.StartsWith(typeMapping.WebApiTypeName);

            var doesAttributeMatch = typeMapping.TreatAsAttribute
                && Helpers.HasCustomAttribute(Parameter, $"{typeMapping.WebApiTypeName}Attribute");

            var doesConstraintMatch = typeMapping.TreatAsConstraint
                && Constraints.Any(c => c == Helpers.ToCamelCaseFromPascalCase(typeMapping.WebApiTypeName));

            var typeMatches = doesTypeNameMatch || doesAttributeMatch || doesConstraintMatch;

            var matchExists = !string.IsNullOrEmpty(typeMapping.Match);
            var doesPatternMatch = matchExists && new Regex(typeMapping.Match).IsMatch(parameterName);

            return (typeMatches && !matchExists)
                || (typeMatches && doesPatternMatch);
        }
    }
}