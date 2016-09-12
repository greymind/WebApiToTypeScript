namespace Endpoints {
    export class AngularEndpointsService {
        static $inject = ['$http'];
        static $http: ng.IHttpService;
    
        constructor($http: ng.IHttpService) {
            AngularEndpointsService.$http = $http;
        }
    
        static call<TView>(endpoint: IEndpoint, data) {
            var call = AngularEndpointsService.$http<TView>({
                method: endpoint._verb,
                url: endpoint.toString(),
                data: data
            });
        
            return call.then(response => response.data);
        }
    
        public Test = {
            Get: (args: Endpoints.Test.IGet): Endpoints.Test.IGetWithCall => {
                var endpoint = new Endpoints.Test.Get(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                });
            },
        
            Get1: (args: Endpoints.Test.IGet1): Endpoints.Test.IGet1WithCall => {
                var endpoint = new Endpoints.Test.Get1(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                });
            },
        
            GetSomething: (args: Endpoints.Test.IGetSomething): Endpoints.Test.IGetSomethingWithCall => {
                var endpoint = new Endpoints.Test.GetSomething(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                });
            },
        
            GetSomethingElse: (args: Endpoints.Test.IGetSomethingElse): Endpoints.Test.IGetSomethingElseWithCall => {
                var endpoint = new Endpoints.Test.GetSomethingElse(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                });
            },
        
            Post: (args: Endpoints.Test.IPost): Endpoints.Test.IPostWithCall => {
                var endpoint = new Endpoints.Test.Post(args);
                return _.extendOwn(endpoint, {
                    call<TView>(value: Interfaces.DummyClass) {
                        return AngularEndpointsService.call<TView>(this, value != null ? value : null);
                    }
                });
            },
        
            Put: (args: Endpoints.Test.IPut): Endpoints.Test.IPutWithCall => {
                var endpoint = new Endpoints.Test.Put(args);
                return _.extendOwn(endpoint, {
                    call<TView>(value: string) {
                        return AngularEndpointsService.call<TView>(this, value != null ? `"${value}"` : null);
                    }
                });
            },
        
            Delete: (args: Endpoints.Test.IDelete): Endpoints.Test.IDeleteWithCall => {
                var endpoint = new Endpoints.Test.Delete(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                });
            }
        }
    }
}
