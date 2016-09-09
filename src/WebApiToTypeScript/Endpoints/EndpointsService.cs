using System.Linq;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript.Endpoints
{
    public class EndpointsService : ServiceAware
    {
        public TypeScriptBlock CreateEndpointBlock()
        {
            return new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.EndpointsNamespace}")
                .AddAndUseBlock($"export interface {IEndpoint}")
                .AddStatement("verb: string;")
                .AddStatement("toString(): string;")
                .Parent
                .AddAndUseBlock($"export interface {IHaveQueryParams}")
                .AddStatement("getQueryParams(): Object")
                .Parent;
        }

        public void WriteEndpointClassToBlock(TypeScriptBlock endpointBlock, WebApiController webApiController)
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
                .AddStatement($"call<TView>({callArgumentsList}): ng.IPromise<TView>;");
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
    }
}
