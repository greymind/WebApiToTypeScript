using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebApiToTypeScript.Types
{
    public class TypeService
    {
        private Dictionary<string, List<Type>> PrimitiveTypesMapping { get; }
            = new Dictionary<string, List<Type>>();

        private List<TypeDefinition> Types { get; }
            = new List<TypeDefinition>();

        public TypeService()
        {
            LoadPrimitiveTypesMapping();
        }

        private void LoadPrimitiveTypesMapping()
        {
            var mapping = PrimitiveTypesMapping;

            mapping["string"] = new List<Type> { typeof(string), typeof(System.Guid), typeof(DateTime) };
            mapping["boolean"] = new List<Type> { typeof(bool) };
            mapping["number"] = new List<Type> { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) };
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

        private IEnumerable<TypeReference> GetBaseTypes(TypeDefinition type)
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

            var nullableType = StripNullable(type);

            var collectionType = StripCollection(type);
            result.IsCollection = collectionType != null;

            result.TypeDefinition = GetTypeDefinition(nullableType ?? collectionType ?? type.FullName);

            return result;
        }

        public string GetPrimitiveTypeScriptType(string typeName)
        {
            return PrimitiveTypesMapping
                .Select(m => m.Value.Any(t => t.FullName == typeName) ? m.Key : string.Empty)
                .SingleOrDefault(name => !string.IsNullOrEmpty(name));
        }

        public bool IsPrimitiveTypeScriptType(string typeName)
        {
            return PrimitiveTypesMapping.Keys
                .Contains(typeName);
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

        public string StripCollection(TypeReference type)
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
    }
}