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

        public override bool Execute()
        {
            Config = GetConfig(ConfigFilePath);

            CreateOuputDirectory();

            var webApiApplicationModule = ModuleDefinition
                .ReadModule(Config.WebApiApplicationAssembly);

            var apiControllers = webApiApplicationModule.GetTypes()
                .Where(IsControllerType());

            var moduleOrNamespace = Config.WriteAsModule ? "module" : "namespace";

            var endpointBlock = new TypeScriptBlock($"{moduleOrNamespace} {Config.Namespace}")
                .AddAndUseBlock($"export interface {IHaveQueryParams}")
                .AddStatement("getQueryParams(): Object")
                .Parent;

            foreach (var apiController in apiControllers)
                WriteEndpointClass(endpointBlock, apiController);

            CreateEndpointFile(endpointBlock);

            return true;
        }

        private void WriteEndpointClass(TypeScriptBlock endpointBlock,
            TypeDefinition apiController)
        {
            var webApiController = new WebApiController(apiController);

            var moduleBlock = endpointBlock
                .AddAndUseBlock($"export namespace {webApiController.Name}");

            var actions = webApiController.Actions;

            foreach (var action in actions)
            {
                var method = action.Method;

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
            var baseTypeScriptTypes = new[] { "string", "number", "boolean" };

            var queryStringParameters = action.QueryStringParameters;

            if (!queryStringParameters.Any())
                return;

            var queryStringBlock = classBlock
                .AddAndUseBlock("private getQueryString = (): string =>")
                .AddStatement("let parameters: string[] = []")
                .AddNewLine();

            foreach (var parameter in queryStringParameters)
            {
                var isOptional = parameter.IsOptional;
                var parameterName = parameter.Name;

                var block = !isOptional
                    ? queryStringBlock
                    : queryStringBlock
                        .AddAndUseBlock($"if (this.{parameterName} != null)");

                if (parameter.HasCustomAttributes
                    && parameter.CustomAttributes.Any(a => a.AttributeType.Name == "FromUriAttribute")
                    && !baseTypeScriptTypes.Contains(GetTypeScriptType(parameter)))
                {
                    block
                        .AddStatement($"let {parameterName}Params = this.{parameterName}.getQueryParams();")
                        .AddAndUseBlock($"Object.keys({parameterName}Params).forEach((key) =>", isFunctionBlock: true)
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

            var constructorParameterStrings = constructorParameters
                .Select(GetParameterStrings(true))
                .Select(p => $"public {p}");

            var constructorParametersList =
                string.Join(", ", constructorParameterStrings);

            classBlock
                .AddAndUseBlock($"constructor({constructorParametersList})");
        }

        private Func<ParameterDefinition, string> GetParameterStrings(
            bool processOptional = false)
        {
            return p => $"{p.Name}{(processOptional && p.IsOptional ? "?" : "")}: {GetTypeScriptType(p)}";
        }

        private string GetTypeScriptType(ParameterDefinition parameter)
        {
            var typeName = parameter.ParameterType.FullName;

            var typeMapping = Config.TypeMappings
                .SingleOrDefault(t => t.WebApiTypeName == typeName
                    || (t.TreatAsAttribute
                        && parameter.HasCustomAttributes
                        && parameter.CustomAttributes.Any(a => a.AttributeType.Name == t.WebApiTypeName)));

            if (typeMapping != null)
                return typeMapping.TypeScriptTypeName;

            var parameterTypeDefinition = parameter.ParameterType as TypeDefinition;
            if (parameterTypeDefinition?.BaseType.FullName == "System.Enum")
                return "number";

            switch (typeName)
            {
                case "System.String":
                    return "string";

                case "System.Int32":
                    return "number";

                case "System.Boolean":
                    return "boolean";

                default:
                    return $"{IHaveQueryParams}"
            ;
            }
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

        private void CreateEndpointFile(TypeScriptBlock endpointBlock)
        {
            var endpointFileName = Config.EndpointFileName ?? "Endpoints.ts";

            var endpointFilePath = Path.Combine(Config.OutputDirectory, endpointFileName);
            using (var endpointFileWriter = new StreamWriter(endpointFilePath, false))
            {
                endpointFileWriter.Write(endpointBlock.ToString());
            }

            LogMessage($"{endpointFilePath} created!");
        }

        private void CreateOuputDirectory()
        {
            if (!Directory.Exists(Config.OutputDirectory))
            {
                Directory.CreateDirectory(Config.OutputDirectory);
                LogMessage($"{Config.OutputDirectory} created!");
            }
            else
            {
                LogMessage($"{Config.OutputDirectory} already exists!");
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