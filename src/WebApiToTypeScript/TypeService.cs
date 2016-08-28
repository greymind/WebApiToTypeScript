using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace WebApiToTypeScript
{
    public class TypeService
    {
        private string WebApiModuleFilePath { get; }

        private Dictionary<string, List<Type>> PrimitiveTypesMapping { get; }
            = new Dictionary<string, List<Type>>();

        private List<TypeDefinition> Types { get; }
            = new List<TypeDefinition>();

        public TypeService(string webApiModuleFilePath)
        {
            WebApiModuleFilePath = webApiModuleFilePath;

            LoadPrimitiveTypesMapping();
        }

        private void LoadPrimitiveTypesMapping()
        {
            var mapping = PrimitiveTypesMapping;

            mapping["string"] = new List<Type> { typeof(string), typeof(System.Guid), typeof(DateTime) };
            mapping["boolean"] = new List<Type> { typeof(bool) };
            mapping["number"] = new List<Type> { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) };
        }

        public void LoadAllTypes()
        {
            var webApiApplicationModule = ModuleDefinition
                .ReadModule(WebApiModuleFilePath);

            var corlib = ModuleDefinition.ReadModule(typeof(object).Module.FullyQualifiedName);
            Types.AddRange(corlib.GetTypes());

            Types.AddRange(webApiApplicationModule.GetTypes());

            var moduleDirectoryName = Path.GetDirectoryName(WebApiModuleFilePath);

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

        public List<TypeDefinition> GetControllers()
        {
            var webApiApplicationModule = ModuleDefinition
                .ReadModule(WebApiModuleFilePath);

            return webApiApplicationModule.GetTypes()
                .Where(IsControllerType)
                .ToList();
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

        public string GetPrimitiveType(string typeName)
        {
            return PrimitiveTypesMapping
                .Select(m => m.Value.Any(t => t.FullName == typeName) ? m.Key : string.Empty)
                .SingleOrDefault(name => !string.IsNullOrEmpty(name));
        }

        public bool IsPrimitiveType(string typeName)
        {
            return PrimitiveTypesMapping.Keys
                .Contains(typeName);
        }
    }
}