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
    public class WebApiToTypeScript : Task
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
                Outer = "module Endpoints"
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

            endpointBlock.WithBlock($"export class {endpointName}Endpoint");
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