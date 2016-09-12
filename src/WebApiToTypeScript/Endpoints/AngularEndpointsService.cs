using WebApiToTypeScript.Block;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript.Endpoints
{
    public class AngularEndpointsService : ServiceAware
    {
        public TypeScriptBlock CreateServiceBlock()
        {
            return new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.ServiceNamespace}")
                .AddAndUseBlock($"export class {Config.ServiceName}")
                .AddStatement("static $inject = ['$http'];")
                .AddStatement("static $http: ng.IHttpService;")
                .AddAndUseBlock("constructor($http: ng.IHttpService)")
                .AddStatement($"{Config.ServiceName}.$http = $http;")
                .Parent
                .AddAndUseBlock("static call<TView>(endpoint: IEndpoint, data)")
                .AddAndUseBlock($"var call = {Config.ServiceName}.$http<TView>(", isFunctionBlock: true, terminationString: ";")
                .AddStatement("method: endpoint._verb,")
                .AddStatement("url: endpoint.toString(),")
                .AddStatement("data: data")
                .Parent
                .AddStatement("return call.then(response => response.data);")
                .Parent
                .Parent;
        }

        public void WriteServiceObjectToBlock(TypeScriptBlock serviceBlock, WebApiController webApiController)
        {
            var controllerBlock = serviceBlock
                .AddAndUseBlock($"public {webApiController.Name} =");

            var actions = webApiController.Actions;

            for (var a = 0; a < actions.Count; a++)
            {
                var action = actions[a];

                for (var v = 0; v < action.Verbs.Count; v++)
                {
                    var verb = action.Verbs[v];

                    var actionName = action.GetActionNameForVerb(verb);

                    var isLastActionAndVerb = a == actions.Count - 1
                        && v == action.Verbs.Count - 1;

                    var constructorParametersList = action.GetConstructorParametersList();
                    var constructorParameterNamesList = action.GetConstructorParameterNamesList();

                    var callArgumentDefinition = action.GetCallArgumentDefinition(verb);
                    var callArgumentValue = action.GetCallArgumentValue(verb);

                    var interfaceFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.I{actionName}";
                    var interfaceWithCallFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.I{actionName}WithCall";
                    var endpointFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.{actionName}";

                    controllerBlock
                        .AddAndUseBlock
                        (
                            outer: $"{actionName}: (args: {interfaceFullName}): {interfaceWithCallFullName} =>",
                            isFunctionBlock: false,
                            terminationString: !isLastActionAndVerb ? "," : string.Empty
                        )
                        .AddStatement($"var endpoint = new {endpointFullName}(args);")
                        .AddAndUseBlock("return _.extendOwn(endpoint,", isFunctionBlock: true, terminationString: ";")
                        .AddAndUseBlock($"call<TView>({callArgumentDefinition})")
                        .AddStatement($"return {Config.ServiceName}.call<TView>(this, {callArgumentValue});");
                }
            }
        }
    }
}