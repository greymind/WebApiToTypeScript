using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Newtonsoft.Json;

namespace WebApiToTypeScript
{
    public class WebApiToTypeScript : AppDomainIsolatedTask
    {
        private const string IHaveQueryParams = nameof(IHaveQueryParams);

        private TypeService typeService;

        [Required]
        public string ConfigFilePath { get; set; }

        public Config Config { get; set; }

        private List<TypeDefinition> Enums { get; }
            = new List<TypeDefinition>();

        private List<TypeDefinition> Interfaces { get; }
            = new List<TypeDefinition>();

        public override bool Execute()
        {
            Config = GetConfig(ConfigFilePath);

            typeService = new TypeService(Config.WebApiModuleFileName);
            typeService.LoadAllTypes();

            var apiControllers = typeService.GetControllers();

            var moduleOrNamespace = Config.WriteNamespaceAsModule ? "module" : "namespace";

            var endpointBlock = new TypeScriptBlock($"{moduleOrNamespace} {Config.EndpointsNamespace}")
                .AddAndUseBlock($"export interface {IHaveQueryParams}")
                .AddStatement("getQueryParams(): Object")
                .Parent;

            foreach (var apiController in apiControllers)
                WriteEndpointClass(endpointBlock, apiController);

            CreateFileForBlock(endpointBlock, Config.EndpointsOutputDirectory, Config.EndpointsFileName);

            var enumsBlock = Config.GenerateEnums
                ? new TypeScriptBlock($"{moduleOrNamespace} {Config.EnumsNamespace}")
                : new TypeScriptBlock();

            if (Config.GenerateEnums)
            {
                foreach (var typeDefinition in Enums)
                    CreateEnumForType(enumsBlock, typeDefinition);
            }

            if (Config.GenerateInterfaces)
            {
                var interfacesBlock = new TypeScriptBlock($"{moduleOrNamespace} {Config.InterfacesNamespace}");

                foreach (var typeDefinition in Interfaces.ToList())
                    CreateInterfaceForType(interfacesBlock, enumsBlock, typeDefinition);

                CreateFileForBlock(interfacesBlock, Config.InterfacesOutputDirectory, Config.InterfacesFileName);
            }

            if (Config.GenerateEnums)
                CreateFileForBlock(enumsBlock, Config.EnumsOutputDirectory, Config.EnumsFileName);

            return true;
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
                foreach (var verb in action.Verbs)
                {
                    var classBlock = moduleBlock
                        .AddAndUseBlock($"export class {action.Name}")
                        .AddStatement($"verb: string = '{verb.VerbMethod}';");

                    CreateConstructorBlock(classBlock, webApiController.RouteParts, action);

                    CreateQueryStringBlock(classBlock, action);

                    CreateToStringBlock(classBlock, webApiController.BaseEndpoint, action);

                    CreateCallBlock(classBlock, action, verb);
                }
            }
        }

        private void CreateCallBlock(TypeScriptBlock classBlock, WebApiAction action,
            WebApiHttpVerb verb)
        {
            var isFormBody = verb == WebApiHttpVerb.Post || verb == WebApiHttpVerb.Put;
            var dataDelimiter = isFormBody ? "," : string.Empty;

            var callArguments = action.BodyParameters;

            var callArgumentStrings = callArguments
                .Select(a => GetParameterString(a, false));

            var callArgumentsList = string.Join(",", callArgumentStrings);

            var callBlock = classBlock
                .AddAndUseBlock($"call = ({callArgumentsList}) =>")
                .AddStatement("let $http = angular.injector(['ng']).get('$http');")
                .AddAndUseBlock("return $http(", isFunctionBlock: true)
                .AddStatement($"method: '{verb.VerbMethod}',")
                .AddStatement($"url: `${{this.toString()}}`{dataDelimiter}");

            if (isFormBody)
            {
                foreach (var argument in callArguments)
                {
                    var typeScriptType = GetTypeScriptType(argument);
                    var valueFormat = $"{argument.Name}";

                    switch (typeScriptType)
                    {
                        case "string":
                            valueFormat = $"`\"${{{argument.Name}}}\"`";
                            break;
                    }

                    callBlock
                        .AddStatement($"data: {argument.Name} != null ? {valueFormat} : null");
                }
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

                if (parameter.CustomAttributes.Any(a => a == "FromUriAttribute")
                    && !typeService.IsPrimitiveType(GetTypeScriptType(parameter)))
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
            var tempConstructorParameters = action.Method.Parameters
                .Select(p => new
                {
                    Parameter = p,
                    RoutePart = baseRouteParts.SingleOrDefault(brp => brp.ParameterName == p.Name)
                        ?? action.RouteParts.SingleOrDefault(rp => rp.ParameterName == p.Name)
                        ?? action.QueryStringParameters.SingleOrDefault(qsp => qsp.Name == p.Name)
                })
                .Where(cp => cp.RoutePart != null)
                .ToList();

            var constructorParameters = new List<WebApiRoutePart>();
            foreach (var tcp in tempConstructorParameters)
            {
                if (tcp.RoutePart.Parameter == null)
                    tcp.RoutePart.Parameter = tcp.Parameter;

                constructorParameters.Add(tcp.RoutePart);
            }

            if (!constructorParameters.Any())
                return;

            var constructorParameterMappings = constructorParameters
                .Select(routePart => new
                {
                    IsOptional = IsParameterOptional(routePart.Parameter),
                    TypeMapping = GetTypeMapping(routePart),
                    Name = routePart.Parameter.Name,
                    String = GetParameterString(routePart)
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

        private string GetParameterString(WebApiRoutePart routePart, bool withOptionals = true)
        {
            var parameter = routePart.Parameter;
            var isOptional = withOptionals && IsParameterOptional(parameter);

            return $"{parameter.Name}{(isOptional ? "?" : "")}: {GetTypeScriptType(routePart)}";
        }

        private bool IsParameterOptional(ParameterDefinition parameter)
        {
            return parameter.IsOptional || !parameter.ParameterType.IsValueType || IsNullable(parameter.ParameterType);
        }

        private string GetTypeScriptType(WebApiRoutePart routePart)
        {
            var parameter = routePart.Parameter;
            var type = parameter.ParameterType;
            var typeName = type.FullName;

            var typeMapping = GetTypeMapping(routePart);

            if (typeMapping != null)
                return typeMapping.TypeScriptTypeName;

            typeName = StripNullable(type) ?? typeName;

            var typeDefinition = typeService.GetTypeDefinition(typeName);

            if (typeDefinition?.IsEnum ?? false)
            {
                if (!Config.GenerateEnums)
                    return "number";

                if (Enums.All(e => e.FullName != typeDefinition.FullName))
                    Enums.Add(typeDefinition);

                return $"{Config.EnumsNamespace}.{typeDefinition.Name}";
            }

            var primitiveType = typeService.GetPrimitiveType(typeName);

            if (!string.IsNullOrEmpty(primitiveType))
                return primitiveType;

            if (!typeDefinition?.IsValueType ?? false)
            {
                if (!Config.GenerateInterfaces)
                    return $"{IHaveQueryParams}";

                if (Interfaces.All(i => i.FullName != typeDefinition.FullName))
                    Interfaces.Add(typeDefinition);

                return $"{Config.InterfacesNamespace}.{typeDefinition.Name}";
            }

            throw new Exception("We shouldn't get here?");
        }

        private TypeMapping GetTypeMapping(WebApiRoutePart routePart)
        {
            if (routePart.Parameter == null)
                return null;

            var parameter = routePart.Parameter;
            var typeName = parameter.ParameterType.FullName;

            var typeMapping = Config.TypeMappings
                .SingleOrDefault(t => typeName.StartsWith(t.WebApiTypeName)
                    || (t.TreatAsAttribute
                        && (Helpers.HasCustomAttribute(parameter, $"{t.WebApiTypeName}Attribute"))
                    || (t.TreatAsConstraint
                        && routePart.Constraints.Any(c => c == Helpers.ToCamelCase(t.WebApiTypeName)))));

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

        private void CreateEnumForType(TypeScriptBlock enumsBlock, TypeDefinition typeDefinition)
        {
            var fields = typeDefinition.Fields
                .Where(f => f.HasConstant && !f.IsSpecialName);

            var enumBlock = enumsBlock
                .AddAndUseBlock($"export enum {typeDefinition.Name}");

            foreach (var field in fields)
                enumBlock.AddStatement($"{field.Name} = {field.Constant},");
        }

        private void CreateInterfaceForType(TypeScriptBlock interfacesBlock,
            TypeScriptBlock enumsBlock, TypeDefinition typeDefinition)
        {
            var fields = typeDefinition.Fields
                .Where(f => f.IsPublic && !f.IsSpecialName && !f.IsStatic)
                .Select(f => new
                {
                    f.Name,
                    Type = typeService.GetTypeDefinition(f.FieldType.FullName)
                });

            var properties = typeDefinition.Properties
                .Where(p => !p.IsSpecialName)
                .Select(p => new
                {
                    p.Name,
                    Type = typeService.GetTypeDefinition(p.PropertyType.FullName)
                });

            var things = fields.Union(properties)
                .Where(t => t.Type != null);

            if (!things.Any())
                return;

            var interfaceBlock = interfacesBlock
                .AddAndUseBlock($"export class {typeDefinition.Name}");

            foreach (var thing in things)
            {
                var primitiveType = typeService.GetPrimitiveType(thing.Type.FullName);
                if (primitiveType != null)
                {
                    interfaceBlock.AddStatement($"{thing.Name}: {primitiveType};");
                }
                else
                {
                    if (thing.Type == null)
                        continue;

                    if (thing.Type.IsEnum && Config.GenerateEnums)
                    {
                        if (Enums.All(e => e.FullName != thing.Type.FullName))
                        {
                            Enums.Add(thing.Type);
                            CreateEnumForType(enumsBlock, thing.Type);
                        }

                        interfaceBlock.AddStatement($"{thing.Name}: {Config.EnumsNamespace}.{thing.Type.Name};");
                    }
                    else if (!thing.Type.IsPrimitive)
                    {
                        if (Interfaces.All(i => i.FullName != thing.Type.FullName))
                        {
                            Interfaces.Add(thing.Type);
                            CreateInterfaceForType(interfacesBlock, enumsBlock, thing.Type);
                        }

                        interfaceBlock.AddStatement($"{thing.Name}: {thing.Type.Name};");
                    }
                }
            }

            interfaceBlock
                .AddAndUseBlock($"getQueryParams()")
                .AddStatement($"return this;");
        }

        private Config GetConfig(string configFilePath)
        {
            var configFileContent = File.ReadAllText(configFilePath);

            return JsonConvert.DeserializeObject<Config>(configFileContent);
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