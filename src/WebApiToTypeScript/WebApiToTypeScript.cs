using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.Enums;
using WebApiToTypeScript.Interfaces;
using WebApiToTypeScript.Types;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript
{
    public class WebApiToTypeScript : AppDomainIsolatedTask
    {
        public const string IHaveQueryParams = nameof(IHaveQueryParams);
        public const string IEndpoint = nameof(IEndpoint);
        public const string AngularEndpointsService = nameof(AngularEndpointsService);

        public static readonly TypeService TypeService
            = new TypeService();

        public static InterfaceService InterfaceService;
        public static EnumsService EnumsService;

        [Required]
        public string ConfigFilePath { get; set; }

        public static Config.Config Config { get; set; }

        public override bool Execute()
        {
            Config = GetConfig(ConfigFilePath);

            TypeService.LoadAllTypes(Config.WebApiModuleFileName);

            EnumsService = new EnumsService();
            InterfaceService = new InterfaceService();

            var apiControllers = TypeService.GetControllers(Config.WebApiModuleFileName);

            var endpointBlock = CreateEndpointBlock();

            var serviceBlock = CreateServiceBlock();

            foreach (var apiController in apiControllers)
            {
                var webApiController = new WebApiController(apiController);

                WriteEndpointClassToBlock(endpointBlock, webApiController);
                WriteServiceObjectToBlock(serviceBlock.Children.First() as TypeScriptBlock, webApiController);
            }

            CreateFileForBlock(endpointBlock, Config.EndpointsOutputDirectory, Config.EndpointsFileName);
            CreateFileForBlock(serviceBlock, Config.ServiceOutputDirectory, Config.ServiceFileName);

            var enumsBlock = Config.GenerateEnums
                ? new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.EnumsNamespace}")
                : new TypeScriptBlock();

            var interfacesBlock =
                new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.InterfacesNamespace}");

            if (Config.GenerateInterfaces)
            {
                InterfaceService.WriteInterfacesToBlock(interfacesBlock);

                CreateFileForBlock(interfacesBlock, Config.InterfacesOutputDirectory, Config.InterfacesFileName);
            }

            if (Config.GenerateEnums)
            {
                EnumsService.WriteEnumsToBlock(enumsBlock);

                CreateFileForBlock(enumsBlock, Config.EnumsOutputDirectory, Config.EnumsFileName);
            }

            return true;
        }

        private TypeScriptBlock CreateEndpointBlock()
        {
            return new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.EndpointsNamespace}")
                .AddAndUseBlock($"export interface {IEndpoint}")
                .AddStatement("verb: string")
                .AddStatement("toString(): string")
                .Parent
                .AddAndUseBlock($"export interface {IHaveQueryParams}")
                .AddStatement("getQueryParams(): Object")
                .Parent;
        }

        private TypeScriptBlock CreateServiceBlock()
        {
            return new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.ServiceNamespace}")
                .AddAndUseBlock($"export class {AngularEndpointsService}")
                .AddStatement("static $inject = ['$http'];")
                .AddStatement("static $http: ng.IHttpService;")
                .AddAndUseBlock("constructor($http: ng.IHttpService)")
                .AddStatement($"{AngularEndpointsService}.$http = $http;")
                .Parent
                .AddAndUseBlock("static call(endpoint: IEndpoint, data)")
                .AddAndUseBlock($"return {AngularEndpointsService}.$http(", isFunctionBlock: true, terminationString: ";")
                .AddStatement("method: endpoint.verb,")
                .AddStatement("url: endpoint.toString(),")
                .AddStatement("data: data")
                .Parent
                .Parent
                .Parent;
        }

        private void WriteServiceObjectToBlock(TypeScriptBlock serviceBlock, WebApiController webApiController)
        {
            var controllerBlock = serviceBlock
                .AddAndUseBlock($"public {webApiController.Name} =");

            var actions = webApiController.Actions;

            for (var a = 0; a < actions.Count; a++)
            {
                var action = actions[a];

                for (var v = 0; v < action.Verbs.Count; v++)
                {
                    var verb = action.Verbs[v];

                    var actionName = action.GetActionNameForVerb(verb);

                    var isLastActionAndVerb = a == actions.Count - 1
                        && v == action.Verbs.Count - 1;

                    var constructorParametersList = action.GetConstructorParametersList();
                    var constructorParameterNamesList = action.GetConstructorParameterNamesList();

                    var callArgumentDefinition = action.GetCallArgumentDefinition(verb);
                    var callArgumentValue = action.GetCallArgumentValue(verb);

                    var interfaceFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.I{actionName}";
                    var endpointFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.{actionName}";

                    controllerBlock
                        .AddAndUseBlock
                        (
                            outer: $"{actionName}: ({constructorParametersList}): {interfaceFullName} =>",
                            isFunctionBlock: false,
                            terminationString: !isLastActionAndVerb ? "," : string.Empty
                        )
                        .AddStatement($"var endpoint = new {endpointFullName}({constructorParameterNamesList});")
                        .AddAndUseBlock("var callHook =")
                        .AddAndUseBlock($"call({callArgumentDefinition})")
                        .AddStatement($"return {AngularEndpointsService}.call(this, {callArgumentValue});")
                        .Parent
                        .Parent
                        .AddStatement("return _.extend(endpoint, callHook);");
                }
            }
        }

        private void WriteEndpointClassToBlock(TypeScriptBlock endpointBlock, WebApiController webApiController)
        {
            var controllerBlock = endpointBlock
                .AddAndUseBlock($"export {Config.NamespaceOrModuleName} {webApiController.Name}");

            var actions = webApiController.Actions;

            foreach (var action in actions)
            {
                foreach (var verb in action.Verbs)
                {
                    var actionName = action.GetActionNameForVerb(verb);

                    var interfaceBlock = controllerBlock
                        .AddAndUseBlock($"export interface I{actionName} extends {IEndpoint}");

                    WriteInterfaceToBlock(interfaceBlock, action);

                    var classBlock = controllerBlock
                        .AddAndUseBlock($"export class {actionName} implements {IEndpoint}")
                        .AddStatement($"verb = '{verb.VerbMethod}';");

                    WriteConstructorToBlock(classBlock, action);

                    WriteGetQueryStringToBlock(classBlock, action);

                    WriteToStringToBlock(classBlock, action);
                }
            }
        }

        private void WriteInterfaceToBlock(TypeScriptBlock interfaceBlock, WebApiAction action)
        {
            var constructorParameterMappings = action.GetConstructorParameterMappings();
            foreach (var constructorParameterMapping in constructorParameterMappings)
            {
                interfaceBlock
                    .AddStatement($"{constructorParameterMapping.String};");
            }

            var callArguments = action.BodyParameters;

            var callArgumentStrings = callArguments
                .Select(a => a.GetParameterString(false))
                .ToList();

            var callArgumentsList = string.Join(", ", callArgumentStrings);

            interfaceBlock
                .AddStatement($"call({callArgumentsList});");
        }

        private void WriteToStringToBlock(TypeScriptBlock classBlock, WebApiAction action)
        {
            var toStringBlock = classBlock
                .AddAndUseBlock("toString = (): string =>");

            var queryString = action.QueryStringParameters.Any()
                ? " + this.getQueryString()"
                : string.Empty;

            toStringBlock
                .AddStatement($"return `{action.Controller.BaseEndpoint}{action.Endpoint}`{queryString};");
        }

        private void WriteGetQueryStringToBlock(TypeScriptBlock classBlock, WebApiAction action)
        {
            var queryStringParameters = action.QueryStringParameters;

            if (!queryStringParameters.Any())
                return;

            var queryStringBlock = classBlock
                .AddAndUseBlock("private getQueryString = (): string =>")
                .AddStatement("var parameters: string[] = [];");

            foreach (var routePart in queryStringParameters)
            {
                var argumentName = routePart.Name;

                var block = queryStringBlock
                    .AddAndUseBlock($"if (this.{argumentName} != null)");

                var argumentType = routePart.GetTypeScriptType();

                if (argumentType.IsPrimitive || argumentType.IsEnum)
                {
                    if (argumentType.IsCollection)
                    {
                        block
                            .AddStatement($"parameters.push(`{argumentName}=${{this.{argumentName}.join(',')}}`);");
                    }
                    else
                    {
                        block
                            .AddStatement($"parameters.push(`{argumentName}=${{encodeURIComponent(this.{argumentName}.toString())}}`);");
                    }
                }
                else
                {
                    block
                        .AddStatement($"var {argumentName}Params = this.{argumentName}.getQueryParams();")
                        .AddAndUseBlock($"Object.keys({argumentName}Params).forEach((key) =>", isFunctionBlock: true, terminationString: ";")
                        .AddAndUseBlock($"if ({argumentName}Params[key] != null)")
                        .AddStatement($"parameters.push(`${{key}}=${{encodeURIComponent({argumentName}Params[key].toString())}}`);");
                }
            }

            queryStringBlock
                .AddAndUseBlock("if (parameters.length > 0)")
                .AddStatement("return '?' + parameters.join('&');")
                .Parent
                .AddStatement("return '';");
        }

        private void WriteConstructorToBlock(TypeScriptBlock classBlock, WebApiAction action)
        {
            var constructorParameterMappings = action.GetConstructorParameterMappings();

            if (!constructorParameterMappings.Any())
                return;

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

        private Config.Config GetConfig(string configFilePath)
        {
            var configFileContent = File.ReadAllText(configFilePath);

            return JsonConvert.DeserializeObject<Config.Config>(configFileContent);
        }

        private void CreateFileForBlock(TypeScriptBlock typeScriptBlock, string outputDirectory, string fileName)
        {
            CreateOuputDirectory(outputDirectory);

            var filePath = Path.Combine(outputDirectory, fileName);

            using (var endpointFileWriter = new StreamWriter(filePath, false))
            {
                endpointFileWriter.Write(typeScriptBlock.ToString());
            }

            LogMessage($"{filePath} created!");
        }

        private void CreateOuputDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                LogMessage($"{directory} created!");
            }
            else
            {
                LogMessage($"{directory} already exists!");
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