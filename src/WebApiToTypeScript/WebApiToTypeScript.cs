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
            var endpointName = apiController.Name
                .Replace("Controller", string.Empty);

            var endpointRouteName =
                GetEndpointRouteName(apiController) ?? endpointName;

            var moduleBlock = endpointBlock
                .AddAndUseBlock($"export module {endpointName}Endpoint");

            var httpVerbs = new string[] {
                "HttpGetAttribute",
                "HttpPostAttribute",
                "HttpPutAttribute",
                "HttpDeleteAttribute"
            };

            var methods = apiController.Methods
                .Where(m => m.IsPublic
                    && m.HasCustomAttributes
                    && m.CustomAttributes.Any(a => httpVerbs.Contains(a.AttributeType.Name)));

            var methodNames = new HashSet<string>();

            foreach (var method in methods)
            {
                var methodName = GetUniqueMethodName(methodNames, method.Name);

                var methodRouteName = GetMethodRouteName(method) ?? method.Name;

                var classBlock = moduleBlock
                    .AddAndUseBlock($"export class {methodName}");

                CreateAllParameters(classBlock, method);

                CreateConstructorBlock(classBlock, method);

                CreateQueryStringBlock(classBlock, method);

                CreateToStringBlock(classBlock, endpointRouteName, methodRouteName,
                    method);
            }
        }

        private string GetMethodRouteName(MethodDefinition method)
        {
            return method.CustomAttributes
                ?.SingleOrDefault(a => a.AttributeType.Name == "RouteAttribute")
                ?.ConstructorArguments
                .First()
                .Value
                .ToString();
        }

        private string GetEndpointRouteName(TypeDefinition apiController)
        {
            return apiController.CustomAttributes
                ?.SingleOrDefault(a => a.AttributeType.Name == "RoutePrefixAttribute")
                ?.ConstructorArguments
                .First()
                .Value
                .ToString();
        }

        private void CreateToStringBlock(TypeScriptBlock classBlock, string endpointRouteName,
            string methodRouteName, MethodDefinition method)
        {
            var toStringBlock = classBlock
                .AddAndUseBlock("toString(): string");

            var baseEndpoint = $"/{endpointRouteName}/{methodRouteName}";
            var queryString = method.Parameters.Any()
                ? " + this.getQueryString();"
                : string.Empty;

            toStringBlock
                .AddStatement($"return '{baseEndpoint}'{queryString};");
        }

        private void CreateQueryStringBlock(TypeScriptBlock classBlock, MethodDefinition method)
        {
            var allParameters = method.Parameters;

            if (!allParameters.Any())
                return;

            var queryStringBlock = classBlock
                .AddAndUseBlock("private getQueryString(): string")
                .AddStatement("var parameters: string[]")
                .AddNewLine();

            foreach (var parameter in allParameters)
            {
                var isFromBody = parameter.HasCustomAttributes
                    && parameter.CustomAttributes.Any(a => a.AttributeType.Name == "FromBodyAttribute");

                if (isFromBody)
                    continue;

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

        private string GetUniqueMethodName(HashSet<string> methodNames, string originalMethodName)
        {
            var methodName = originalMethodName;

            var counter = 1;
            while (methodNames.Contains(methodName))
                methodName = $"{originalMethodName}{counter++}";

            methodNames.Add(methodName);

            return methodName;
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