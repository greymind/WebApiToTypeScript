using System.Diagnostics;
using System.Linq;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript.Endpoints
{
    public class MobileEndpointsService : ServiceAware
    {
        public TypeScriptBlock CreateEndpointBlock()
        {
            var block = new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.MobileEndpointsNamespace}");

            block
                .AddAndUseBlock($"export interface {IEndpoint}")
                .AddStatement("_verb: string;")
                .AddStatement("toString(): string;")
                .Parent
                .AddAndUseBlock("function addParameter(parameters: string[], key: string, value: any)")
                .AddAndUseBlock("if (value == null)")
                .AddStatement("return;")
                .Parent
                .AddAndUseBlock($"if (Array.isArray(value))")
                .AddStatement($"var encodedItems = value.map((item: any) => encodeURIComponent(item.toString()));")
                .AddStatement($"encodedItems.forEach(item => parameters.push(`${{key}}=${{item}}`));")
                .Parent
                .AddAndUseBlock("else if (value && typeof value === 'object' && value.constructor === Object && value.getQueryParams)")
                .AddStatement(@"addParameter(parameters, key, value.getQueryParams());")
                .Parent
                .AddAndUseBlock("else if (value && typeof value === 'object' && value.constructor === Object)")
                .AddStatement(@"Object.keys(value).forEach((key) => { addParameter(parameters, key, value[key]); });")
                .Parent
                .AddAndUseBlock("else")
                .AddStatement($"parameters.push(`${{key}}=${{encodeURIComponent(value.toString())}}`);");

            return block;
        }

        public void WriteEndpointClassToBlock(TypeScriptBlock endpointsBlock, WebApiController webApiController)
        {
            var controllerBlock = endpointsBlock
                .AddAndUseBlock($"export {Config.NamespaceOrModuleName} {webApiController.Name}");

            var actions = webApiController.Actions.Where(a => a.IsMobileAction);

            foreach (var action in actions)
            {
                if (action.BodyParameters.Count > 1)
                {
                    LogMessage($"Multiple conflicting call parameters detected in action [{action.Name}] of controller [{webApiController.Name}]!");
                    LogMessage("Please use [FromBody] or [FromUri] on all non-primitives!");

                    var parameters = action.BodyParameters.Select(bp => $"[{bp.Name}]");
                    LogMessage($"Parameters: {string.Join(" ", parameters)}");
                    LogMessage("");

                    continue;
                }

                foreach (var verb in action.Verbs)
                {
                    var actionName = action.GetActionNameForVerb(verb);

                    var interfaceBlock = controllerBlock
                        .AddAndUseBlock($"export interface I{actionName}");

                    WriteInterfaceToBlock(interfaceBlock, action);

                    var endpointBlock = controllerBlock
                        .AddAndUseBlock($"export interface I{actionName}Endpoint extends I{actionName}, IEndpoint");

                    var ctorBlock = controllerBlock
                        .AddAndUseBlock($"export interface I{actionName}Ctor")
                        .AddStatement($"new(args?: I{actionName}): I{actionName}Endpoint");
                    
                    var ctorImplBlock = controllerBlock
                        .AddAndUseBlock($"export var {actionName} : I{actionName}Ctor = <any>(function(args?: I{actionName})", false, ");")
                        .AddStatement($"this._verb = '{verb.VerbMethod}';");

                    var constructorParameterMappings = action.GetConstructorParameterMappings(ignoreEnumDefinitions: true);
                    foreach (var mapping in constructorParameterMappings)
                    {
                        ctorImplBlock.AddStatement($"this.{mapping.Name} = args != null ? args.{mapping.Name} : null;");
                        if (mapping.TypeMapping?.AutoInitialize ?? false)
                        {
                            ctorImplBlock
                                .AddAndUseBlock($"if (this.{mapping.Name} == null)")
                                .AddStatement($"this.{mapping.Name} = new {mapping.TypeMapping.TypeScriptTypeName}();");
                        }
                    }
                    
                    WriteGetQueryStringToBlock(controllerBlock, actionName, action);

                    WriteToStringToBlock(controllerBlock, actionName, action);
                }
            }
        }

        private void WriteInterfaceToBlock(TypeScriptBlock interfaceBlock, WebApiAction action)
        {
            var constructorParameterMappings = action.GetConstructorParameterMappings(ignoreEnumDefinitions: true);
            foreach (var constructorParameterMapping in constructorParameterMappings)
            {
                interfaceBlock
                    .AddStatement($"{constructorParameterMapping.StringWithOptionals};");
            }
        }

        private void WriteToStringToBlock(TypeScriptBlock classBlock, string actionName, WebApiAction action)
        {
            var toStringBlock = classBlock
                .AddAndUseBlock($"{actionName}.prototype.toString = function(): string");

            var queryString = action.QueryStringParameters.Any()
                ? " + this.getQueryString()"
                : string.Empty;

            toStringBlock
                .AddStatement($"return `{action.Controller.BaseEndpoint}{action.Endpoint}`{queryString};");
        }

        private void WriteGetQueryStringToBlock(TypeScriptBlock classBlock, string actionName, WebApiAction action)
        {
            var queryStringParameters = action.QueryStringParameters;

            if (!queryStringParameters.Any())
                return;

            var queryStringBlock = classBlock
                .AddAndUseBlock($"{actionName}.prototype.getQueryString = function(): string")
                .AddStatement("var parameters: string[] = [];");

            foreach (var routePart in queryStringParameters)
            {
                var argumentName = routePart.Name;

                var block = queryStringBlock;

                var argumentType = routePart.GetTypeScriptType();

                block.AddStatement($"addParameter(parameters, '{argumentName}', this.{argumentName});");
            }

            queryStringBlock
                .AddAndUseBlock("if (parameters.length > 0)")
                .AddStatement("return '?' + parameters.join('&');")
                .Parent
                .AddStatement("return '';");
        }
    }
}