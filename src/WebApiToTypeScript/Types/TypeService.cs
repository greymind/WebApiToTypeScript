using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WebApiToTypeScript.Config;

namespace WebApiToTypeScript.Types
{
    public class TypeService : ServiceAware
    {
        private Regex ValidTypeNameRegex { get; }
            = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$");

        private Dictionary<string, List<Type>> PrimitiveTypesMapping { get; }
            = new Dictionary<string, List<Type>>();

        private List<string> ReservedWords { get; set; }
            = new List<string>();

        public List<TypeDefinition> Types { get; }
            = new List<TypeDefinition>();

        public TypeService()
        {
            LoadPrimitiveTypesMapping();
            LoadReservedWords();
        }

        public bool IsReservedWord(string word)
        {
            return ReservedWords.Contains(word);
        }

        public string FixIfReservedWord(string word)
        {
            return IsReservedWord(word)
                ? $"_{word}"
                : word;
        }

        public bool IsValidTypeName(string typeName)
        {
            return ValidTypeNameRegex.IsMatch(typeName);
        }

        public void LoadAllTypes(string webApiModuleFilePath)
        {
            var webApiApplicationModule = ModuleDefinition
                .ReadModule(webApiModuleFilePath);

            var corlib = ModuleDefinition.ReadModule(typeof(object).Module.FullyQualifiedName);
            Types.AddRange(corlib.GetTypes());

            Types.AddRange(webApiApplicationModule.GetTypes());

            var moduleDirectoryName = Path.GetDirectoryName(webApiModuleFilePath);

            foreach (var reference in webApiApplicationModule.AssemblyReferences)
            {
                var fileName = $"{reference.Name}.dll";
                var path = Path.Combine(moduleDirectoryName, fileName);

                if (!File.Exists(path))
                    continue;

                var moduleDefinition = ModuleDefinition.ReadModule(path);
                Types.AddRange(moduleDefinition.GetTypes());
            }
        }

        public List<TypeDefinition> GetControllers(string webApiModuleFilePath)
        {
            var webApiApplicationModule = ModuleDefinition
                .ReadModule(webApiModuleFilePath);

            return webApiApplicationModule.GetTypes()
                .Where(IsControllerType)
                .ToList();
        }

        public bool IsNullable(TypeReference type)
        {
            var genericType = type as GenericInstanceType;
            return genericType != null
                   && genericType.FullName.StartsWith("System.Nullable`1");
        }

        public bool IsParameterOptional(ParameterDefinition parameter)
        {
            return parameter.IsOptional
                || !parameter.ParameterType.IsValueType
                || IsNullable(parameter.ParameterType);
        }

        private bool IsControllerType(TypeDefinition type)
        {
            var apiControllerType = "System.Web.Http.ApiController";

            return type.IsClass
                && !type.IsAbstract
                && type.Name.EndsWith("Controller")
                && GetBaseTypes(type).Any(bt => bt.FullName == apiControllerType);
        }

        public IEnumerable<TypeReference> GetBaseTypes(TypeDefinition type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;

                var baseTypeDefinition = baseType as TypeDefinition;
                baseType = baseTypeDefinition?.BaseType;
            }
        }

        public TypeDefinition GetTypeDefinition(string typeName)
        {
            return Types
                .FirstOrDefault(t => t.FullName == typeName);
        }

        public CSharpType GetCSharpType(TypeReference type)
        {
            var result = new CSharpType();

            result.IsValueType = type.IsValueType;

            var nullableType = StripNullable(type);
            result.IsNullable = nullableType != null;

            var collectionType = StripCollection(type);
            result.IsCollection = collectionType != null;

            result.IsGenericParameter = type.IsGenericParameter;
            result.GenericParameterName = type.Name;

            result.TypeDefinition = GetTypeDefinition(nullableType ?? collectionType ?? type.FullName);

            return result;
        }

        public TypeScriptType GetTypeScriptType(TypeReference cSharpType, string parameterName)
        {
            return GetTypeScriptType(cSharpType, parameterName, GetTypeMapping);
        }

        private TypeScriptType MapTypeMappingToTypeScriptType(TypeMapping typeMapping, TypeScriptType typeScriptType)
        {
            var typeScriptTypeName = typeMapping.TypeScriptTypeName;

            typeScriptType.TypeName = typeScriptTypeName;
            typeScriptType.InterfaceName = typeScriptTypeName;
            typeScriptType.IsPrimitive = IsPrimitiveTypeScriptType(typeScriptType.TypeName);
            typeScriptType.IsEnum = typeScriptTypeName.StartsWith($"{Config.EnumsNamespace}")
                || typeScriptType.IsPrimitive;

            return typeScriptType;
        }

        public TypeScriptType GetTypeScriptType(TypeReference cSharpType, string parameterName, Func<string, string, TypeMapping> getTypeMapping)
        {
            var result = new TypeScriptType();

            var type = StripTask(cSharpType) ?? cSharpType;

            var typeName = type.FullName;

            {
                var typeMapping = getTypeMapping(parameterName, typeName);

                if (typeMapping != null)
                {
                    return MapTypeMappingToTypeScriptType(typeMapping, result);
                }
            }

            var strippedTypeName = StripNullable(type);

            if (strippedTypeName != null)
            {
                var typeMapping = getTypeMapping(parameterName, strippedTypeName);

                if (typeMapping != null)
                {
                    return MapTypeMappingToTypeScriptType(typeMapping, result);
                }
            }

            typeName = strippedTypeName ?? typeName;

            var collectionTypeName = StripCollection(type);

            if (collectionTypeName != null)
            {
                result.IsCollection = true;

                var typeMapping = getTypeMapping(parameterName, collectionTypeName);

                if (typeMapping != null)
                {
                    return MapTypeMappingToTypeScriptType(typeMapping, result);
                }
            }

            typeName = collectionTypeName ?? typeName;

            var typeDefinition = GetTypeDefinition(typeName);

            if (typeDefinition?.IsEnum ?? false)
            {
                if (!Config.GenerateEnums)
                {
                    result.TypeName = "number";
                    result.InterfaceName = "number";
                    result.IsPrimitive = true;
                }
                else
                {
                    EnumsService.AddEnum(typeDefinition);

                    result.TypeName = $"{Config.EnumsNamespace}.{typeDefinition.Name}";
                    result.InterfaceName = $"{Config.EnumsNamespace}.{typeDefinition.Name}";
                    result.IsPrimitive = false;
                }

                result.IsEnum = true;
                return result;
            }

            var primitiveType = GetPrimitiveTypeScriptType(typeName);

            if (!string.IsNullOrEmpty(primitiveType))
            {
                result.TypeName = primitiveType;
                result.InterfaceName = primitiveType;
                result.IsPrimitive = true;

                return result;
            }

            if (!typeDefinition?.IsValueType ?? false)
            {
                if (!Config.GenerateInterfaces)
                {
                    result.TypeName = $"any";
                    result.InterfaceName = "any";
                }
                else
                {
                    InterfaceService.AddInterfaceNode(typeDefinition);

                    result.TypeName = $"{Config.InterfacesNamespace}.{typeDefinition.Name}";
                    result.InterfaceName = $"{Config.InterfacesNamespace}.I{typeDefinition.Name}";
                }

                return result;
            }

            var logTypeName = typeDefinition?.FullName ?? type.FullName;
            var isValueType = typeDefinition?.IsValueType ?? type.IsValueType;

            LogMessage($"Parameter [{parameterName}] of type [{logTypeName}] unmapped. IsValueType: [{isValueType}]");
            result.TypeName = "any";
            result.InterfaceName = "any";
            result.IsPrimitive = isValueType;

            return result;

            throw new NotSupportedException("Maybe it is a generic class, or a yet unsupported collection, or chain thereof?");
        }

        private TypeMapping GetTypeMapping(string parameterName, string typeFullName)
        {
            var typeMapping = Config.TypeMappings
                .FirstOrDefault(t => MatchTypeMapping(parameterName, typeFullName, t));

            return typeMapping;
        }

        private bool MatchTypeMapping(string parameterName, string typeFullName, TypeMapping typeMapping)
        {
            var doesTypeNameMatch = typeFullName.StartsWith(typeMapping.WebApiTypeName);

            var matchExists = !string.IsNullOrEmpty(typeMapping.Match);
            var doesPatternMatch = matchExists && new Regex(typeMapping.Match).IsMatch(parameterName);

            return (doesTypeNameMatch && !matchExists)
                || (doesTypeNameMatch && doesPatternMatch);
        }

        public string GetPrimitiveTypeScriptType(string typeFullName)
        {
            return PrimitiveTypesMapping
                .Select(m => m.Value.Any(t => t.FullName == typeFullName) ? m.Key : string.Empty)
                .SingleOrDefault(name => !string.IsNullOrEmpty(name));
        }

        public bool IsPrimitiveTypeScriptType(string typeScriptTypeName)
        {
            return PrimitiveTypesMapping.Keys
                .Contains(typeScriptTypeName);
        }

        public string StripNullable(TypeReference type)
        {
            var genericType = type as GenericInstanceType;
            if (genericType != null
                && genericType.FullName.StartsWith("System.Nullable`1")
                && genericType.HasGenericArguments
                && genericType.GenericArguments.Count == 1)
            {
                return genericType.GenericArguments.Single().FullName;
            }

            return null;
        }

        private string StripCollection(TypeReference type)
        {
            if (type.IsArray)
            {
                return type.GetElementType().FullName;
            }

            var genericCollectionTypes = new[]
            {
                "System.Collections.Generic.IList`1",
                "System.Collections.Generic.List`1",
                "System.Collections.Generic.IEnumerable`1",
                "System.Collections.Generic.Enumerable`1"
            };

            var genericType = type as GenericInstanceType;
            if (genericType != null
                && genericCollectionTypes.Any(gct => genericType.FullName.StartsWith(gct))
                && genericType.HasGenericArguments
                && genericType.GenericArguments.Count == 1)
            {
                return genericType.GenericArguments.Single().FullName;
            }

            return null;
        }

        public TypeReference StripTask(TypeReference type)
        {
            var taskType = "System.Threading.Tasks.Task";

            var genericType = type as GenericInstanceType;
            if (genericType != null && genericType.FullName.StartsWith(taskType))
            {
                return genericType.GenericArguments.Single();
            }
            else if (type.FullName.StartsWith(taskType))
            {
                return this.Types.First(x => x.FullName == "System.Void");
            }

            return null;
        }

        private void LoadReservedWords()
        {
            ReservedWords = new List<string>
            {
                "constructor"
            };
        }

        private void LoadPrimitiveTypesMapping()
        {
            var mapping = PrimitiveTypesMapping;

            mapping["string"] = new List<Type> { typeof(string), typeof(Guid), typeof(DateTime), typeof(TimeSpan) };
            mapping["boolean"] = new List<Type> { typeof(bool) };
            mapping["number"] = new List<Type> { typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) };
            mapping["any"] = new List<Type> { typeof(object) };
            mapping["void"] = new List<Type> { typeof(void) };
        }
    }
}