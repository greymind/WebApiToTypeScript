using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace WebApiToTypeScript
{
    public class WebApiToTypeScript : AppDomainIsolatedTask
    {
        [Required]
        public string WebApiApplicationAssembly { get; set; }

        [Required]
        public string OutputDirectory { get; set; }

        public string EndpointFileName { get; set; }

        public override bool Execute()
        {
            CreateOuputDirectory();

            var endpointSource = new StringBuilder();

            var webApiApplicationModule = ModuleDefinition.ReadModule(WebApiApplicationAssembly);
            var apiControllers = webApiApplicationModule.GetTypes()
                .Where(IsControllerType());

            var endpointBlock = new TypeScriptBlock
            {
                Outer = "namespace Endpoints"
            };

            foreach (var apiController in apiControllers)
            {
                WriteEndpointClass(endpointBlock, apiController);
            }

            endpointSource.Append(endpointBlock.ToString());

            CreateEndpointFile(endpointSource);

            return true;
        }

        private void WriteEndpointClass(TypeScriptBlock endpointBlock, TypeDefinition apiController)
        {
            var webApiController = new WebApiController(apiController);

            var moduleBlock = endpointBlock
                .AddAndUseBlock($"export module {webApiController.Name}Endpoint");

            var actions = webApiController.Actions;

            foreach (var action in actions)
            {
                var method = action.Method;

                var classBlock = moduleBlock
                    .AddAndUseBlock($"export class {action.Name}");

                CreateAllParameters(classBlock, method);

                CreateConstructorBlock(classBlock, method);

                CreateQueryStringBlock(classBlock, webApiController.RouteParts, action);

                CreateToStringBlock(classBlock, webApiController.BaseEndpoint, action);
            }
        }

        private void CreateToStringBlock(TypeScriptBlock classBlock,
            string baseEndpoint, WebApiAction action)
        {
            var toStringBlock = classBlock
                .AddAndUseBlock("toString(): string");

            var queryString = action.QueryStringParameters.Any()
                ? " + this.getQueryString();"
                : string.Empty;

            toStringBlock
                .AddStatement($"return `{baseEndpoint}`{queryString};");
        }

        private void CreateQueryStringBlock(TypeScriptBlock classBlock,
            List<WebApiRoutePart> baseRouteParts, WebApiAction action)
        {
            var queryStringParameters = action.QueryStringParameters;

            if (!queryStringParameters.Any())
                return;

            var queryStringBlock = classBlock
                .AddAndUseBlock("private getQueryString(): string")
                .AddStatement("var parameters: string[]")
                .AddNewLine();

            foreach (var parameter in queryStringParameters)
            {
                var isOptional = parameter.IsOptional;
                var parameterName = parameter.Name;

                var block = !isOptional
                    ? queryStringBlock
                    : queryStringBlock
                        .AddAndUseBlock($"if (this.{parameterName} != null)");

                block
                    .AddStatement($"parameters.push(`{parameterName}=${{this.{parameterName}}}`);");
            }

            queryStringBlock
                .AddAndUseBlock("if (parameters.length > 0)")
                .AddStatement("return '?' + parameters.join('&');")
                .Parent
                .AddStatement("return '';");
        }

        private void CreateAllParameters(TypeScriptBlock classBlock, MethodDefinition method)
        {
            var allParameters = method.Parameters
                .Select(GetParameterStrings());

            foreach (var parameter in allParameters)
                classBlock.AddStatement(parameter);
        }

        private void CreateConstructorBlock(TypeScriptBlock classBlock, MethodDefinition method)
        {
            var constructorParameters = method.Parameters
                .Where(p => !p.IsOptional);

            if (!constructorParameters.Any())
                return;

            var constructorParameterStrings = constructorParameters
                .Select(GetParameterStrings());

            var constructorParametersList = string.Join(", ", constructorParameterStrings);

            var constructorBlock = classBlock.AddAndUseBlock($"constructor({constructorParametersList})");

            foreach (var constructorParameter in constructorParameters)
            {
                var parameterName = constructorParameter.Name;

                constructorBlock
                    .AddStatement($"this.{parameterName} = {parameterName};");
            }
        }

        private Func<ParameterDefinition, string> GetParameterStrings()
            => p => $"{p.Name}: {GetTypeScriptType(p.ParameterType)}";

        private string GetTypeScriptType(TypeReference parameterType)
        {
            switch (parameterType.FullName)
            {
                case "System.String":
                    return "string";

                case "System.Int32":
                    return "number";

                default:
                    return "any";
            }
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

        private void CreateEndpointFile(StringBuilder endpointSource)
        {
            EndpointFileName = "Endpoints.ts";

            var endpointFilePath = Path.Combine(OutputDirectory, EndpointFileName);
            using (var endpointFileWriter = new StreamWriter(endpointFilePath, false))
            {
                endpointFileWriter.Write(endpointSource.ToString());
            }

            Log.LogMessage($"{endpointFilePath} created!");
        }

        private void CreateOuputDirectory()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
                Log.LogMessage($"{OutputDirectory} created!");
            }
            else
            {
                Log.LogMessage($"{OutputDirectory} already exists!");
            }
        }
    }
}