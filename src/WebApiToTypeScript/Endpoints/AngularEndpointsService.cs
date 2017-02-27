using System.Linq;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript.Endpoints
{
    public class AngularEndpointsService : ServiceAware
    {
        public TypeScriptBlock CreateServiceBlock()
        {
            var constructorBlock = new TypeScriptBlock($"{Config.NamespaceOrModuleName} {Config.ServiceNamespace}")
                .AddAndUseBlock($"export class {Config.ServiceName}")
                .AddStatement
                (
                    Config.EndpointsSupportCaching
                        ? "static $inject = ['$http', '$q'];"
                        : "static $inject = ['$http'];"
                )
                .AddStatement("static endpointCache = {};", condition: Config.EndpointsSupportCaching)
                .AddAndUseBlock
                (
                    Config.EndpointsSupportCaching
                        ? "constructor($http: ng.IHttpService, $q: ng.IQService)"
                        : "constructor($http: ng.IHttpService)"
                );

            var serviceBlock = constructorBlock
                .Parent
                .AddAndUseBlock("static call<TView>(httpService: ng.IHttpService, endpoint: IEndpoint, data, httpConfig?: ng.IRequestShortcutConfig)")
                .AddAndUseBlock("const config = ")
                .AddStatement("method: endpoint._verb,")
                .AddStatement("url: endpoint.toString(),")
                .AddStatement("data: data")
                .Parent
                .AddStatement("httpConfig && _.extend(config, httpConfig);")
                .AddStatement("")
                .AddStatement($"const call = httpService<TView>(config);")
                .AddStatement("return call.then(response => response.data);");

            if (Config.EndpointsSupportCaching)
            {
                serviceBlock
                    .Parent
                    .AddAndUseBlock("static callCached<TView>(httpService: ng.IHttpService, qService: ng.IQService, endpoint: IEndpoint, data, httpConfig?: ng.IRequestShortcutConfig)")
                    .AddStatement("var cacheKey = endpoint.toString();")
                    .AddAndUseBlock("if (this.endpointCache[cacheKey] == null)")
                    .AddAndUseBlock("return this.call<TView>(httpService, endpoint, data, httpConfig).then(result =>", isFunctionBlock: true, terminationString: ";")
                    .AddStatement("this.endpointCache[cacheKey] = result;")
                    .AddStatement("return this.endpointCache[cacheKey];")
                    .Parent
                    .Parent
                    .AddStatement("const deferred = qService.defer();")
                    .AddStatement("deferred.resolve(this.endpointCache[cacheKey]);")
                    .AddStatement("return deferred.promise;");
            }

            return serviceBlock
                .Parent
                .Parent;
        }

        public void WriteServiceObjectToBlock(TypeScriptBlock serviceBlock, WebApiController webApiController)
        {
            var constructorBlock = serviceBlock.Children
                .OfType<TypeScriptBlock>()
                .First();

            var controllerBlock = serviceBlock
                .AddStatement($"public {webApiController.Name}: {Config.EndpointsNamespace}.{webApiController.Name}.I{webApiController.Name}Service = <any>{{}};");

            var actions = webApiController.Actions;

            foreach (var action in actions)
            {
                var constructorParameterMappings = action.GetConstructorParameterMappings();

                foreach (var verb in action.Verbs)
                {
                    var actionName = action.GetActionNameForVerb(verb);

                    var areAllParametersOptional = constructorParameterMappings
                        .All(m => m.IsOptional);

                    var optionalString = areAllParametersOptional
                        ? "?"
                        : string.Empty;

                    var callArgumentDefinition = action.GetCallArgumentDefinition(verb);
                    var callArgumentValue = action.GetCallArgumentValue(verb);

                    var interfaceFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.I{actionName}";
                    var interfaceWithCallFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.I{actionName}WithCall";
                    var endpointFullName = $"{Config.EndpointsNamespace}.{webApiController.Name}.{actionName}";

                    string typeScriptReturnType, typeScriptTypeForCall;

                    action.GetReturnTypes(out typeScriptReturnType, out typeScriptTypeForCall);

                    var endpointExtendBlock = constructorBlock
                        .AddAndUseBlock
                        (
                            outer: $"this.{webApiController.Name}.{actionName} = (args{optionalString}: {interfaceFullName}): {interfaceWithCallFullName} =>",
                            isFunctionBlock: false,
                            terminationString: ";"
                        )
                        .AddStatement($"var endpoint = new {endpointFullName}(args);")
                        .AddAndUseBlock("return _.extendOwn(endpoint,", isFunctionBlock: true, terminationString: ";")
                        .AddAndUseBlock
                        (
                            outer: $"call{typeScriptTypeForCall}({callArgumentDefinition})",
                            isFunctionBlock: false,
                            terminationString: Config.EndpointsSupportCaching ? "," : string.Empty
                        )
                        .AddStatement($"return {Config.ServiceName}.call{typeScriptReturnType}($http, this, {callArgumentValue});")
                        .Parent;

                    if (Config.EndpointsSupportCaching && verb == WebApiHttpVerb.Get)
                    {
                        endpointExtendBlock.AddAndUseBlock($"callCached{typeScriptTypeForCall}({callArgumentDefinition})")
                            .AddStatement($"return {Config.ServiceName}.callCached{typeScriptReturnType}($http, $q, this, {callArgumentValue});");
                    }
                }
            }
        }
    }
}