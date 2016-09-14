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
                .AddAndUseBlock("function isIHaveQueryParams(obj: any): obj is IHaveQueryParams")
                .AddStatement("return !_.isUndefined((<IHaveQueryParams>obj).getQueryParams);")
                .Parent

                .AddAndUseBlock("function addParameter(parameters: string[], key: string, value: any)")
                .AddAndUseBlock("if (value == null)")
                .AddStatement("return;")
                .Parent
                .AddAndUseBlock($"else if (_.isArray(value))")
                .AddStatement($"var encodedItems = _.map(value, (item) => encodeURIComponent(item.toString()));")
                .AddStatement($"parameters.push(`${{key}}=${{encodedItems.join(',')}}`);")
                .Parent
                .AddAndUseBlock("else")
                .AddStatement($"parameters.push(`${{key}}=${{encodeURIComponent(value.toString())}}`);")
                .Parent
                .Parent

                .AddAndUseBlock("function addObjectParameter(parameters: string[], obj: IHaveQueryParams)")
                .AddAndUseBlock("if (obj == null)")
                .AddStatement("return;")
                .Parent
                .AddStatement("var params = obj.getQueryParams();")
                .AddAndUseBlock($"Object.keys(params).forEach((key) =>", isFunctionBlock: true, terminationString: ";")
                .AddAndUseBlock($"if (isIHaveQueryParams(params[key]))")
                .AddStatement("addObjectParameter(parameters, params[key]);")
                .Parent
                .AddAndUseBlock("else")
                .AddStatement("addParameter(parameters, key, params[key]);")
                .Parent
                .Parent
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
                        .AddAndUseBlock($"export interface I{actionName}");

                    WriteInterfaceToBlock(interfaceBlock, action);

                    var interfaceWithCallBlock = controllerBlock
                        .AddAndUseBlock($"export interface I{actionName}WithCall extends I{actionName}, {IEndpoint}");

                    WriteInterfaceWithCallToBlock(interfaceWithCallBlock, action);

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

        private void WriteInterfaceWithCallToBlock(TypeScriptBlock interfaceWithCallBlock, WebApiAction action)
        {
            var callArguments = action.BodyParameters;

            var callArgumentStrings = callArguments
                .Select(a => a.GetParameterString(false))
                .SingleOrDefault();

            var callArgumentsList = string.Join(", ", callArgumentStrings);

            interfaceWithCallBlock
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
                        .AddStatement($"addObjectParameter(parameters, this.{argumentName});");
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