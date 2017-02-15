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
                .AddStatement("_verb: string;")
                .AddStatement("toString(): string;")
                .Parent
                .AddAndUseBlock($"export interface {IHaveQueryParams}")
                .AddStatement("getQueryParams(): Object")
                .Parent

                .AddAndUseBlock("function addParameter(parameters: string[], key: string, value: any)")
                .AddAndUseBlock("if (value == null)")
                .AddStatement("return;")
                .Parent
                .AddAndUseBlock($"if (_.isArray(value))")
                .AddStatement($"var encodedItems = _.map(value, (item: any) => encodeURIComponent(item.toString()));")
                .AddStatement($"_(encodedItems).each(item => parameters.push(`${{key}}=${{item}}`));")
                .Parent
                .AddAndUseBlock("else")
                .AddStatement($"parameters.push(`${{key}}=${{encodeURIComponent(value.toString())}}`);")
                .Parent
                .Parent

                .AddAndUseBlock("function addObjectParameters(parameters: string[], obj: IHaveQueryParams)")
                .AddAndUseBlock("if (obj == null)")
                .AddStatement("return;")
                .Parent
                .AddStatement("var params = obj.getQueryParams();")
                .AddAndUseBlock($"Object.keys(params).forEach((key) =>", isFunctionBlock: true, terminationString: ";")
                .AddStatement("addParameter(parameters, key, params[key]);")
                .Parent
                .Parent;
        }

        public void WriteEndpointClassToBlock(TypeScriptBlock endpointBlock, WebApiController webApiController)
        {
            var controllerBlock = endpointBlock
                .AddAndUseBlock($"export {Config.NamespaceOrModuleName} {webApiController.Name}");

            TypeScriptBlock serviceBlock = null;
            if (Config.GenerateService)
            {
                serviceBlock = controllerBlock
                    .AddAndUseBlock($"export interface I{webApiController.Name}Service");
            }

            var actions = webApiController.Actions;

            foreach (var action in actions)
            {
                foreach (var verb in action.Verbs)
                {
                    var actionName = action.GetActionNameForVerb(verb);

                    var interfaceBlock = controllerBlock
                        .AddAndUseBlock($"export interface I{actionName}");

                    WriteInterfaceToBlock(interfaceBlock, action);

                    if (Config.GenerateService)
                    {
                        var interfaceWithCallBlock = controllerBlock
                            .AddAndUseBlock($"export interface I{actionName}WithCall extends I{actionName}, {IEndpoint}");

                        WriteInterfaceWithCallToBlock(interfaceWithCallBlock, action, verb);

                        serviceBlock
                            .AddStatement($"{actionName}: (args?: I{actionName}) => I{actionName}WithCall");
                    }

                    var classBlock = controllerBlock
                        .AddAndUseBlock($"export class {actionName} implements I{actionName}, {IEndpoint}")
                        .AddStatement($"_verb = '{verb.VerbMethod}';");

                    var constructorParameterMappings = action.GetConstructorParameterMappings();
                    foreach (var constructorParameterMapping in constructorParameterMappings)
                    {
                        classBlock
                            .AddStatement($"{constructorParameterMapping.String};");
                    }

                    WriteConstructorToBlock(classBlock, action, verb);

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
                    .AddStatement($"{constructorParameterMapping.StringWithOptionals};");
            }
        }

        private void WriteInterfaceWithCallToBlock(TypeScriptBlock interfaceWithCallBlock, WebApiAction action, WebApiHttpVerb verb)
        {
            var callArguments = action.BodyParameters;

            var callArgument = callArguments
                .Select(a => a.GetParameterString(withOptionals: false, interfaceName: true))
                .SingleOrDefault();

            var callArgumentsList = string.IsNullOrWhiteSpace(callArgument)
                ? "httpConfig?"
                : $"{callArgument}, httpConfig?";

            string typeScriptReturnType, typeScriptTypeForCall;

            action.GetReturnTypes(out typeScriptReturnType, out typeScriptTypeForCall);

            interfaceWithCallBlock
                .AddStatement($"call{typeScriptTypeForCall}({callArgumentsList}): ng.IPromise{typeScriptReturnType};");

            if (Config.EndpointsSupportCaching && verb == WebApiHttpVerb.Get)
                interfaceWithCallBlock
                    .AddStatement($"callCached{typeScriptTypeForCall}({callArgumentsList}): ng.IPromise{typeScriptReturnType};");
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

                var block = queryStringBlock;

                var argumentType = routePart.GetTypeScriptType();

                if (argumentType.IsPrimitive || argumentType.IsEnum)
                {
                    block
                        .AddStatement($"addParameter(parameters, '{argumentName}', this.{argumentName});");
                }
                else
                {
                    block
                        .AddStatement($"addObjectParameters(parameters, this.{argumentName});");
                }
            }

            queryStringBlock
                .AddAndUseBlock("if (parameters.length > 0)")
                .AddStatement("return '?' + parameters.join('&');")
                .Parent
                .AddStatement("return '';");
        }

        private void WriteConstructorToBlock(TypeScriptBlock classBlock, WebApiAction action, WebApiHttpVerb verb)
        {
            var actionName = action.GetActionNameForVerb(verb);

            var constructorParameterMappings = action.GetConstructorParameterMappings();

            var areAllParametersOptional = constructorParameterMappings
                .All(m => m.IsOptional);

            var optionalString = areAllParametersOptional
                ? "?"
                : string.Empty;

            var constructorBlock = classBlock
                .AddAndUseBlock($"constructor(args{optionalString}: I{actionName})");

            foreach (var mapping in constructorParameterMappings)
            {
                constructorBlock
                    .AddStatement($"this.{mapping.Name} = args != null ? args.{mapping.Name} : null;");

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