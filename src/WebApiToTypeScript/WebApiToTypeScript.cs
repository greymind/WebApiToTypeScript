using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebApiToTypeScript
{
    public class WebApiToTypeScript : AppDomainIsolatedTask
    {
        private const string IHaveQueryParams = nameof(IHaveQueryParams);

        [Required]
        public string ConfigFilePath { get; set; }

        public Config Config { get; set; }

        private List<TypeDefinition> Types { get; }
            = new List<TypeDefinition>();

        private List<TypeDefinition> Enums { get; }
            = new List<TypeDefinition>();

        private Dictionary<string, List<Type>> TypeScriptPrimitiveTypesMapping { get; }
            = new Dictionary<string, List<Type>>();

        public override bool Execute()
        {
            Config = GetConfig(ConfigFilePath);

            LoadTypeScriptPrimitiveTypesMapping();

            CreateOuputDirectory();

            var webApiApplicationModule = ModuleDefinition
                .ReadModule(Config.WebApiModuleFileName);

            AddAllTypes(webApiApplicationModule);

            var apiControllers = webApiApplicationModule.GetTypes()
                .Where(IsControllerType());

            var moduleOrNamespace = Config.WriteNamespaceAsModule ? "module" : "namespace";

            var endpointBlock = new TypeScriptBlock($"{moduleOrNamespace} {Config.EndpointsNamespace}")
                .AddAndUseBlock($"export interface {IHaveQueryParams}")
                .AddStatement("getQueryParams(): Object")
                .Parent;

            foreach (var apiController in apiControllers)
                WriteEndpointClass(endpointBlock, apiController);

            CreateFileForBlock(endpointBlock, Config.EndpointsOutputDirectory, Config.EndpointsFileName);

            if (Config.GenerateEnums)
            {
                var enumsBlock = new TypeScriptBlock($"{moduleOrNamespace} {Config.EnumsNamespace}");

                foreach (var typeDefinition in Enums)
                    CreateEnumForType(enumsBlock, typeDefinition);

                CreateFileForBlock(enumsBlock, Config.EnumsOutputDirectory, Config.EnumsFileName);
            }

            return true;
        }

        private void LoadTypeScriptPrimitiveTypesMapping()
        {
            var mapping = TypeScriptPrimitiveTypesMapping;

            mapping["string"] = new List<Type> { typeof(string), typeof(System.Guid) };
            mapping["boolean"] = new List<Type> { typeof(bool) };
            mapping["number"] = new List<Type> { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) };
        }

        private void AddAllTypes(ModuleDefinition webApiApplicationModule)
        {
            Types.AddRange(webApiApplicationModule.GetTypes());

            var moduleDirectoryName = Path.GetDirectoryName(Config.WebApiModuleFileName);

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

        private void WriteEndpointClass(TypeScriptBlock endpointBlock,
            TypeDefinition apiController)
        {
            var webApiController = new WebApiController(apiController);

            var moduleOrNamespace = Config.WriteNamespaceAsModule ? "module" : "namespace";

            var moduleBlock = endpointBlock
                .AddAndUseBlock($"export {moduleOrNamespace} {webApiController.Name}");

            var actions = webApiController.Actions;

            foreach (var action in actions)
            {
                var classBlock = moduleBlock
                     .AddAndUseBlock($"export class {action.Name}")
                     .AddStatement($"verb: string = '{action.Verb}';");

                CreateConstructorBlock(classBlock, webApiController.RouteParts, action);

                CreateQueryStringBlock(classBlock, action);

                CreateToStringBlock(classBlock, webApiController.BaseEndpoint, action);
            }
        }

        private void CreateToStringBlock(TypeScriptBlock classBlock,
            string baseEndpoint, WebApiAction action)
        {
            var toStringBlock = classBlock
                .AddAndUseBlock("toString = (): string =>");

            var queryString = action.QueryStringParameters.Any()
                ? " + this.getQueryString()"
                : string.Empty;

            toStringBlock
                .AddStatement($"return `{baseEndpoint}{action.Endpoint}`{queryString};");
        }

        private void CreateQueryStringBlock(TypeScriptBlock classBlock, WebApiAction action)
        {
            var queryStringParameters = action.QueryStringParameters;

            if (!queryStringParameters.Any())
                return;

            var queryStringBlock = classBlock
                .AddAndUseBlock("private getQueryString = (): string =>")
                .AddStatement("let parameters: string[] = [];");

            foreach (var parameter in queryStringParameters)
            {
                var parameterName = parameter.Name;

                var block = queryStringBlock
                    .AddAndUseBlock($"if (this.{parameterName} != null)");

                if (parameter.HasCustomAttributes
                    && parameter.CustomAttributes.Any(a => a.AttributeType.Name == "FromUriAttribute")
                    && !TypeScriptPrimitiveTypesMapping.Keys.Contains(GetTypeScriptType(parameter)))
                {
                    block
                        .AddStatement($"let {parameterName}Params = this.{parameterName}.getQueryParams();")
                        .AddAndUseBlock($"Object.keys({parameterName}Params).forEach((key) =>", isFunctionBlock: true)
                        .AddAndUseBlock($"if ({parameterName}Params[key] != null)")
                        .AddStatement($"parameters.push(`${{key}}=${{{parameterName}Params[key]}}`);");
                }
                else
                {
                    block
                        .AddStatement($"parameters.push(`{parameterName}=${{this.{parameterName}}}`);");
                }
            }

            queryStringBlock
                .AddAndUseBlock("if (parameters.length > 0)")
                .AddStatement("return '?' + parameters.join('&');")
                .Parent
                .AddStatement("return '';");
        }

        private void CreateConstructorBlock(TypeScriptBlock classBlock,
            List<WebApiRoutePart> baseRouteParts, WebApiAction action)
        {
            var constructorParameters = action.Method.Parameters
                .Where(p => baseRouteParts.Any(brp => brp.ParameterName == p.Name)
                    || action.RouteParts.Any(rp => rp.ParameterName == p.Name)
                    || action.QueryStringParameters.Any(qsp => qsp.Name == p.Name))
                .ToList();

            if (!constructorParameters.Any())
                return;

            var constructorParameterMappings = constructorParameters
                .Select(p => new
                {
                    IsOptional = IsParameterOptional(p),
                    TypeMapping = GetTypeMapping(p),
                    Name = p.Name,
                    String = GetParameterStringForConstructor(p)
                })
                .OrderBy(p => p.IsOptional);

            var constructorParameterStrings = constructorParameterMappings
                .Select(p => $"public {p.String}");

            var constructorParametersList =
                string.Join(", ", constructorParameterStrings);

            var constructorBlock = classBlock
                .AddAndUseBlock($"constructor({constructorParametersList})");

            foreach (var mapping in constructorParameterMappings)
            {
                if (mapping.TypeMapping?.AutoInitialize ?? false)
                {
                    constructorBlock
                        .AddAndUseBlock($"if (this.{mapping.Name} == null)")
                        .AddStatement($"this.{mapping.Name} = new {mapping.TypeMapping.TypeScriptTypeName}();");
                }
            }
        }

        private string GetParameterStringForConstructor(ParameterDefinition parameter)
        {
            var isOptional = IsParameterOptional(parameter);
            return $"{parameter.Name}{(isOptional ? "?" : "")}: {GetTypeScriptType(parameter)}";
        }

        private bool IsParameterOptional(ParameterDefinition parameter)
        {
            return parameter.IsOptional || !parameter.ParameterType.IsValueType || IsNullable(parameter.ParameterType);
        }

        private string GetTypeScriptType(ParameterDefinition parameter)
        {
            var type = parameter.ParameterType;
            var typeName = type.FullName;

            var typeMapping = GetTypeMapping(parameter);

            if (typeMapping != null)
                return typeMapping.TypeScriptTypeName;

            typeName = StripNullable(type) ?? typeName;

            // TODO Support encrypted long via route {xx:encryptedLong}

            var typeDefinition = Types
                .FirstOrDefault(t => t.FullName == typeName);

            if (typeDefinition?.IsEnum ?? false)
            {
                if (!Config.GenerateEnums)
                    return "number";

                if (Enums.All(e => e.FullName != typeDefinition.FullName))
                    Enums.Add(typeDefinition);

                return $"{Config.EnumsNamespace}.{typeDefinition.Name}";
            }

            var result = TypeScriptPrimitiveTypesMapping
                .Select(m => m.Value.Any(t => t.FullName == typeName) ? m.Key : string.Empty)
                .SingleOrDefault(name => !string.IsNullOrEmpty(name));

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            if (Config.GenerateInterfaces)
            {
            }

            return $"{IHaveQueryParams}";
        }

        private TypeMapping GetTypeMapping(ParameterDefinition parameter)
        {
            var typeName = parameter.ParameterType.FullName;

            var typeMapping = Config.TypeMappings
                .SingleOrDefault(t => typeName.StartsWith(t.WebApiTypeName)
                    || (t.TreatAsAttribute
                            && parameter.HasCustomAttributes
                            && parameter.CustomAttributes.Any(a => a.AttributeType.Name == t.WebApiTypeName)));

            return typeMapping;
        }

        private bool IsNullable(TypeReference type)
        {
            var genericType = type as GenericInstanceType;
            return genericType != null
                   && genericType.FullName.StartsWith("System.Nullable`1");
        }

        private string StripNullable(TypeReference type)
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

        private static void CreateEnumForType(TypeScriptBlock enumsBlock, TypeDefinition typeDefinition)
        {
            var fields = typeDefinition.Fields
                .Where(f => f.HasConstant && !f.IsSpecialName);

            var enumBlock = enumsBlock
                .AddAndUseBlock($"export enum {typeDefinition.Name}");

            foreach (var field in fields)
                enumBlock.AddStatement($"{field.Name} = {field.Constant},");
        }

        private Config GetConfig(string configFilePath)
        {
            var configFileContent = File.ReadAllText(configFilePath);

            return JsonConvert.DeserializeObject<Config>(configFileContent);
        }

        private Func<TypeDefinition, bool> IsControllerType()
        {
            var apiControllerType = "System.Web.Http.ApiController";

            return t => t.IsClass
                && !t.IsAbstract
                && t.Name.EndsWith("Controller")
                && GetBaseTypes(t).Any(bt => bt.FullName == apiControllerType);
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

        private void CreateFileForBlock(TypeScriptBlock typeScriptBlock, string outputDirectory, string fileName)
        {
            var filePath = Path.Combine(outputDirectory, fileName);
            using (var endpointFileWriter = new StreamWriter(filePath, false))
            {
                endpointFileWriter.Write(typeScriptBlock.ToString());
            }

            LogMessage($"{filePath} created!");
        }

        private void CreateOuputDirectory()
        {
            if (!Directory.Exists(Config.EndpointsOutputDirectory))
            {
                Directory.CreateDirectory(Config.EndpointsOutputDirectory);
                LogMessage($"{Config.EndpointsOutputDirectory} created!");
            }
            else
            {
                LogMessage($"{Config.EndpointsOutputDirectory} already exists!");
            }
        }

        private void LogMessage(string log)
        {
            try
            {
                Log.LogMessage(log);
            }
            catch (Exception)
            {
                Console.WriteLine(log);
            }
        }
    }
}