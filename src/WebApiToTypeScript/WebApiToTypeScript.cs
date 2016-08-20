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

            var moduleBlock = endpointBlock.AddAndUseBlock($"export module {endpointName}Endpoint");

            var httpVerbs = new string[] {
                "HttpGetAttribute",
                "HttpPostAttribute",
                "HttpPutAttribute",
                "HttpDeleteAttribute"
            };

            var methods = apiController.Methods
                .Where(m => m.IsPublic
                    && m.HasCustomAttributes
                    && m.CustomAttributes.Any(a => httpVerbs.Contains(a.AttributeType.Name)))
                .Select(m => new
                {
                    MethodDefinition = m,
                    HttpVerb = m.CustomAttributes.Single(a => httpVerbs.Contains(a.AttributeType.Name))
                });

            var methodNames = new HashSet<string>();

            foreach (var method in methods)
            {
                var methodDefinition = method.MethodDefinition;
                var originalMethodName = methodDefinition.Name;
                var verbName = method.HttpVerb.AttributeType.Name;

                var methodName = GetUniqueMethodName(methodNames, originalMethodName);

                var classBlock = moduleBlock
                    .AddAndUseBlock($"export class {methodName}");

                CreateAllParameters(methodDefinition, classBlock);

                CreateConstructorBlock(methodDefinition, classBlock);

                classBlock
                    .AddAndUseBlock("toString(): string")
                    .AddStatement($"//{verbName}")
                    .AddStatement($"return `/api/{endpointName}/{methodName}`;");
            }
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

        private void CreateAllParameters(MethodDefinition methodDefinition, TypeScriptBlock classBlock)
        {
            var allParameters = methodDefinition.Parameters
                .Select(GetParameterStrings());

            foreach (var parameter in allParameters)
                classBlock.AddStatement(parameter);
        }

        private void CreateConstructorBlock(MethodDefinition methodDefinition, TypeScriptBlock classBlock)
        {
            var constructorParameters = methodDefinition.Parameters
                .Where(p => !p.IsOptional);

            if (!constructorParameters.Any())
                return;

            var constructorParameterStrings = constructorParameters
                .Select(GetParameterStrings());

            var constructorParametersList = string.Join(", ", constructorParameters);

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