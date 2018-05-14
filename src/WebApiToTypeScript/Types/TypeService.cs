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
        private readonly Regex ValidTypeNameRegex
            = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$");

        private readonly Regex GenericNameRegEx
            = new Regex("(.+)(`(\\d+))");

        private readonly string[] TypesToTreatAsObjects = new string[]
        {
            "System.Linq.Expressions.Expression`1",
            "System.Collections.Generic.IDictionary`2",
            "System.Collections.Generic.Dictionary`2",
            "System.Func`",
        };

        private Dictionary<string, List<Type>> PrimitiveTypesMapping { get; }
            = new Dictionary<string, List<Type>>();

        private List<string> ReservedWords { get; set; }
            = new List<string>();

        public List<TypeDefinition> Types { get; }
            = new List<TypeDefinition>();

        public TypeDefinition VoidType => this.Types.First(x => x.FullName == "System.Void");
        public TypeDefinition ObjectType => this.Types.First(x => x.FullName == "System.Object");

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

                var baseTypeDefinition = GetTypeDefinition(baseType.FullName);
                baseType = baseTypeDefinition?.BaseType;
            }
        }

        public TypeDefinition GetTypeDefinition(string typeName)
        {
            return Types
                .FirstOrDefault(t => t.FullName == typeName);
        }

        public CSharpType GetCSharpType(TypeReference type, string memberFullName)
        {
            var result = new CSharpType();

            var strippedType = StripGenerics(type, memberFullName, out bool isNullable, out int collectionLevel) ?? type;
            result.IsNullable = isNullable;
            result.CollectionLevel = collectionLevel;

            result.IsValueType = strippedType.IsValueType;

            if (strippedType.IsGenericParameter)
            {
                result.IsGenericParameter = true;
                result.GenericParameterName = strippedType.Name;
            }
            else if (strippedType is GenericInstanceType genericType
                && genericType.HasGenericArguments)
            {
                if (TypesToTreatAsObjects.Any(i => genericType.FullName.StartsWith(i)))
                {
                    result.TypeDefinition = ObjectType;
                }
                else
                {
                    result.IsGenericInstance = true;
                    result.GenericArgumentTypes = genericType.GenericArguments.ToArray();

                    result.TypeDefinition = GetTypeDefinition(genericType.ElementType.FullName);
                }
            }
            else
            {
                result.TypeDefinition = GetTypeDefinition(strippedType.FullName);
            }

            if (!result.IsValid)
            {
                LogMessage($"Cannot get C# type for {memberFullName}!");
            }

            return result;
        }

        public TypeScriptType GetTypeScriptType(TypeReference cSharpType, string parameterName)
        {
            return GetTypeScriptType(cSharpType, parameterName, GetTypeMapping);
        }

        public TypeScriptType GetPrefixedTypeScriptType(TypeReference cSharpType, string parameterName, string interfacePrefix = "Interfaces")
        {
            return GetPrefixedTypeScriptType(cSharpType, parameterName, GetTypeMapping, interfacePrefix);
        }

        public TypeScriptType GetPrefixedTypeScriptType(TypeReference cSharpType, string parameterName, Func<string, string, TypeMapping> getTypeMapping, string interfacePrefix = "Interfaces")
        {
            var typeScriptType = TypeService.GetTypeScriptType(cSharpType, parameterName, getTypeMapping);

            string prefix = "";

            if (typeScriptType.IsEnum && Config.GenerateEnums)
            {
                prefix = Config.NoNamespacesOrModules
                    ? "Enums."
                    : $"{Config.EnumsNamespace}.";
            }
            else if (Config.GenerateInterfaces)
            {
                prefix = Config.NoNamespacesOrModules
                    ? !typeScriptType.IsPrimitive
                        ? !string.IsNullOrEmpty(interfacePrefix)
                            ? $"{interfacePrefix}."
                            : ""
                        : ""
                    : $"{Config.EnumsNamespace}.";
            }

            return new TypeScriptType
            {
                CollectionLevel = typeScriptType.CollectionLevel,
                IsEnum = typeScriptType.IsEnum,
                IsMappedType = typeScriptType.IsMappedType,
                IsPrimitive = typeScriptType.IsPrimitive,
                InterfaceName = $"{prefix}{typeScriptType.InterfaceName}",
                TypeName = $"{prefix}{typeScriptType.TypeName}"
            };
        }

        private TypeScriptType MapTypeMappingToTypeScriptType(TypeMapping typeMapping, TypeScriptType typeScriptType)
        {
            var typeScriptTypeName = typeMapping.TypeScriptTypeName;

            typeScriptType.TypeName = typeScriptTypeName;
            typeScriptType.InterfaceName = typeScriptTypeName;

            typeScriptType.IsPrimitive = IsPrimitiveTypeScriptType(typeScriptType.TypeName);
            typeScriptType.IsEnum = (!Config.NoNamespacesOrModules && typeScriptTypeName.StartsWith($"{Config.EnumsNamespace}"))
                || typeMapping.TreatAsEnum
                || typeScriptType.IsPrimitive;

            typeScriptType.IsMappedType = true;

            return typeScriptType;
        }

        public TypeScriptType GetTypeScriptType(TypeReference cSharpType, string parameterName, Func<string, string, TypeMapping> getTypeMapping)
        {
            var result = new TypeScriptType();

            var type = StripGenerics(cSharpType, parameterName, out bool isNullable, out int collectionLevel);
            result.CollectionLevel = collectionLevel;

            var typeMapping = getTypeMapping(parameterName, type.FullName);

            if (typeMapping != null)
            {
                return MapTypeMappingToTypeScriptType(typeMapping, result);
            }

            var typeName = type.FullName;

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

                    var enumPrefix = !Config.NoNamespacesOrModules
                        ? $"{Config.EnumsNamespace}."
                        : "";

                    result.TypeName = $"{enumPrefix}{typeDefinition.Name}";
                    result.InterfaceName = $"{enumPrefix}{typeDefinition.Name}";
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

            if (typeDefinition != null
                && !typeDefinition.IsPrimitive)
            {
                if (!Config.GenerateInterfaces)
                {
                    result.TypeName = $"any";
                    result.InterfaceName = "any";
                }
                else
                {
                    InterfaceService.AddInterfaceNode(typeDefinition);

                    var cleanTypeName = CleanGenericName(typeDefinition.Name);

                    result.TypeName = $"{cleanTypeName}";
                    result.InterfaceName = $"I{cleanTypeName}";
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

        public TypeMapping GetTypeMapping(string parameterName, string typeFullName)
        {
            if (typeFullName == null)
                return null;

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

        public TypeReference StripGenerics(TypeReference type, string memberFullName, out bool isNullable, out int collectionLevel)
        {
            var genericCollectionTypeFullNames = new[]
            {
                "System.Collections.Generic.IList`1",
                "System.Collections.Generic.List`1",
                "System.Collections.Generic.IEnumerable`1",
                "System.Collections.Generic.Enumerable`1",
                "System.Collections.Generic.ICollection`1",
                "System.Linq.IQueryable`1",
                "System.Collections.Generic.IReadOnlyList`1",
            };

            var genericNullableTypeFullName = "System.Nullable`1";

            var taskTypeFullName = "System.Threading.Tasks.Task";
            var genericTaskTypeFullName = $"{taskTypeFullName}`1";

            isNullable = false;
            collectionLevel = 0;

            var loopType = type;

            while (true)
            {
                if (loopType is GenericInstanceType genericType
                    && genericType.HasGenericArguments)
                {
                    if (TypesToTreatAsObjects.Any(i => memberFullName.StartsWith(i)))
                    {
                        return ObjectType;
                    }
                    else if (genericType.GenericArguments.Count != 1)
                    {
                        LogMessage($"Multiple generic arguments for member [{memberFullName}]. This is currently unsupported!");
                        return ObjectType;
                    }
                    else if (genericCollectionTypeFullNames.Any(gct => genericType.ElementType.FullName == gct))
                    {
                        collectionLevel++;

                        loopType = genericType.GenericArguments.Single();
                        continue;
                    }
                    else if (genericType.ElementType.FullName == genericNullableTypeFullName)
                    {
                        isNullable = true;

                        loopType = genericType.GenericArguments.Single();
                        continue;
                    }
                    else if (genericType.ElementType.FullName == genericTaskTypeFullName)
                    {
                        loopType = genericType.GenericArguments.Single();
                        continue;
                    }
                }
                else if (loopType is ArrayType arrayType)
                {
                    collectionLevel++;

                    loopType = arrayType.ElementType;
                    continue;
                }
                else if (loopType.FullName == taskTypeFullName)
                {
                    return VoidType;
                }

                return loopType;
            }
        }

        public string CleanGenericName(string maybeGenericTypeName)
        {
            return string.IsNullOrEmpty(maybeGenericTypeName)
                ? string.Empty
                : GenericNameRegEx.Replace(maybeGenericTypeName, "$1Generic$3");
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