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
                    var verbPostfix = action.Verbs.Count > 1
                        ? verb == WebApiHttpVerb.Post ? "New" : "Existing"
                        : string.Empty;

                    var classBlock = moduleBlock
                        .AddAndUseBlock($"export class {action.Name}{verbPostfix}")
                        .AddStatement($"verb = '{verb.VerbMethod}';");

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

            var callArguments = action.BodyParameters;

            var callArgumentStrings = callArguments
                .Select(a => GetParameterString(a, false))
                .ToList();

            var callArgumentsList = string.Join(",", callArgumentStrings);

            var dataDelimiter = isFormBody && callArgumentStrings.Any() ? "," : string.Empty;

            var callBlock = classBlock
                .AddAndUseBlock($"call = ({callArgumentsList}) =>")
                .AddStatement("const httpService = angular.injector(['ng']).get<ng.IHttpService>('$http');")
                .AddAndUseBlock("return httpService(", isFunctionBlock: true, terminateWithSemicolon: true)
                .AddStatement($"method: '{verb.VerbMethod}',")
                .AddStatement($"url: this.toString(){dataDelimiter}");

            if (!isFormBody)
                return;

            foreach (var argument in callArguments)
            {
                var typeScriptType = GetTypeScriptType(argument)
                    .TypeName;

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

            foreach (var routePart in queryStringParameters)
            {
                var argumentName = routePart.Name;

                var block = queryStringBlock
                    .AddAndUseBlock($"if (this.{argumentName} != null)");

                var argumentType = GetTypeScriptType(routePart);

                if (argumentType.IsPrimitive || argumentType.IsEnum)
                {
                    block
                        .AddStatement($"parameters.push(`{argumentName}=${{this.{argumentName}}}`);");
                }
                else
                {
                    block
                        .AddStatement($"let {argumentName}Params = this.{argumentName}.getQueryParams();")
                        .AddAndUseBlock($"Object.keys({argumentName}Params).forEach((key) =>", isFunctionBlock: true, terminateWithSemicolon: true)
                        .AddAndUseBlock($"if ({argumentName}Params[key] != null)")
                        .AddStatement($"parameters.push(`${{key}}=${{{argumentName}Params[key]}}`);");
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
            var typeScriptType = GetTypeScriptType(routePart);

            var collectionString = typeScriptType.IsCollection ? "[]" : string.Empty;

            return $"{parameter.Name}{(isOptional ? "?" : "")}: {typeScriptType.TypeName}{collectionString}";
        }

        private bool IsParameterOptional(ParameterDefinition parameter)
        {
            return parameter.IsOptional || !parameter.ParameterType.IsValueType || IsNullable(parameter.ParameterType);
        }

        private TypeScriptType GetTypeScriptType(WebApiRoutePart routePart)
        {
            var result = new TypeScriptType();

            var parameter = routePart.Parameter;
            var type = parameter.ParameterType;
            var typeName = type.FullName;

            var typeMapping = GetTypeMapping(routePart);

            if (typeMapping != null)
            {
                var tsTypeName = typeMapping.TypeScriptTypeName;
                result.TypeName = tsTypeName;
                result.IsPrimitive = typeService.IsPrimitiveType(result.TypeName);
                result.IsEnum = tsTypeName.StartsWith($"{Config.EnumsNamespace}")
                    || result.IsPrimitive;

                return result;
            }

            typeName = typeService.StripNullable(type) ?? typeName;

            var collectionType = typeService.StripCollection(type);
            result.IsCollection = collectionType != null;
            typeName = collectionType ?? typeName;

            var typeDefinition = typeService.GetTypeDefinition(typeName);

            if (typeDefinition?.IsEnum ?? false)
            {
                if (!Config.GenerateEnums)
                {
                    result.TypeName = "number";
                    result.IsPrimitive = true;
                }
                else
                {
                    if (Enums.All(e => e.FullName != typeDefinition.FullName))
                        Enums.Add(typeDefinition);

                    result.TypeName = $"{Config.EnumsNamespace}.{typeDefinition.Name}";
                    result.IsPrimitive = false;
                }

                result.IsEnum = true;
                return result;
            }

            var primitiveType = typeService.GetPrimitiveType(typeName);

            if (!string.IsNullOrEmpty(primitiveType))
            {
                result.TypeName = primitiveType;
                result.IsPrimitive = true;

                return result;
            }

            if (!typeDefinition?.IsValueType ?? false)
            {
                if (!Config.GenerateInterfaces)
                {
                    result.TypeName = $"{IHaveQueryParams}";
                }
                else
                {
                    if (Interfaces.All(i => i.FullName != typeDefinition.FullName))
                        Interfaces.Add(typeDefinition);

                    result.TypeName = $"{Config.InterfacesNamespace}.{typeDefinition.Name}";
                }

                return result;
            }

            throw new NotSupportedException("Maybe it is a generic class, or a yet unsupported collection, or chain thereof?");
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
                    CSharpType = typeService.GetCSharpType(f.FieldType)
                });

            var properties = typeDefinition.Properties
                .Where(p => !p.IsSpecialName && p.SetMethod != null)
                .Select(p => new
                {
                    p.Name,
                    CSharpType = typeService.GetCSharpType(p.PropertyType)
                });

            var things = fields.Union(properties)
                .Where(t => t.CSharpType.TypeDefinition != null)
                .ToList();

            // TODO Handle IEnumerable, BaseType, Collections, Interfaces (base things, generic T)

            if (!things.Any())
                return;

            var interfaceBlock = interfacesBlock
                .AddAndUseBlock($"export class {typeDefinition.Name}");

            foreach (var thing in things)
            {
                var thingType = thing.CSharpType.TypeDefinition;
                var collectionString = thing.CSharpType.IsCollection ? "[]" : string.Empty;

                var primitiveType = typeService.GetPrimitiveType(thingType.FullName);
                if (primitiveType != null)
                {
                    interfaceBlock.AddStatement($"{thing.Name}: {primitiveType}{collectionString};");
                }
                else
                {
                    if (thingType.IsEnum && Config.GenerateEnums)
                    {
                        if (Enums.All(e => e.FullName != thingType.FullName))
                        {
                            Enums.Add(thingType);
                            CreateEnumForType(enumsBlock, thingType);
                        }

                        interfaceBlock.AddStatement($"{thing.Name}: {Config.EnumsNamespace}.{thingType.Name}{collectionString};");
                    }
                    else if (!thingType.IsPrimitive)
                    {
                        if (Interfaces.All(i => i.FullName != thingType.FullName))
                        {
                            Interfaces.Add(thingType);
                            CreateInterfaceForType(interfacesBlock, enumsBlock, thingType);
                        }

                        interfaceBlock.AddStatement($"{thing.Name}: {thingType.Name}{collectionString};");
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