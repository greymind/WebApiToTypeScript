using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using WebApiToTypeScript.Config;
using WebApiToTypeScript.Enums;
using WebApiToTypeScript.Interfaces;
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
            var result = new TypeScriptType();

            var parameter = Parameter;
            var type = parameter.ParameterType;
            var typeName = type.FullName;

            var typeMapping = GetTypeMapping();

            if (typeMapping != null)
            {
                var tsTypeName = typeMapping.TypeScriptTypeName;
                result.TypeName = tsTypeName;
                result.IsPrimitive = TypeService.IsPrimitiveTypeScriptType(result.TypeName);
                result.IsEnum = tsTypeName.StartsWith($"{Config.EnumsNamespace}")
                    || result.IsPrimitive;

                return result;
            }

            typeName = TypeService.StripNullable(type) ?? typeName;

            var collectionType = TypeService.StripCollection(type);
            result.IsCollection = collectionType != null;
            typeName = collectionType ?? typeName;

            var typeDefinition = TypeService.GetTypeDefinition(typeName);

            if (typeDefinition?.IsEnum ?? false)
            {
                if (!Config.GenerateEnums)
                {
                    result.TypeName = "number";
                    result.IsPrimitive = true;
                }
                else
                {
                    EnumsService.AddEnum(typeDefinition);

                    result.TypeName = $"{Config.EnumsNamespace}.{typeDefinition.Name}";
                    result.IsPrimitive = false;
                }

                result.IsEnum = true;
                return result;
            }

            var primitiveType = TypeService.GetPrimitiveTypeScriptType(typeName);

            if (!string.IsNullOrEmpty(primitiveType))
            {
                result.TypeName = primitiveType;
                result.IsPrimitive = true;

                return result;
            }

            if (!typeDefinition?.IsValueType ?? false)
            {
                if (!Config.GenerateInterfaces)
                {
                    result.TypeName = $"{WebApiToTypeScript.IHaveQueryParams}";
                }
                else
                {
                    InterfaceService.AddInterfaceNode(typeDefinition);

                    result.TypeName = $"{Config.InterfacesNamespace}.{typeDefinition.Name}";
                }

                return result;
            }

            throw new NotSupportedException("Maybe it is a generic class, or a yet unsupported collection, or chain thereof?");
        }

        public string GetParameterString(bool withOptionals = true)
        {
            var isOptional = withOptionals && IsOptional && TypeService.IsParameterOptional(Parameter);
            var typeScriptType = GetTypeScriptType();

            var collectionString = typeScriptType.IsCollection ? "[]" : string.Empty;

            return $"{Parameter.Name}{(isOptional ? "?" : "")}: {typeScriptType.TypeName}{collectionString}";
        }

        public TypeMapping GetTypeMapping()
        {
            if (Parameter == null)
                return null;

            var typeName = Parameter.ParameterType.FullName;

            var typeMapping = Config.TypeMappings
                .SingleOrDefault(t => typeName.StartsWith(t.WebApiTypeName)
                    || (t.TreatAsAttribute
                        && (Helpers.HasCustomAttribute(Parameter, $"{t.WebApiTypeName}Attribute"))
                    || (t.TreatAsConstraint
                        && Constraints.Any(c => c == Helpers.ToCamelCase(t.WebApiTypeName)))));

            return typeMapping;
        }
    }
}