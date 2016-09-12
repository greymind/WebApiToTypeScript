namespace Endpoints {
    export class AngularEndpointsService {
        static $inject = ['$http'];
        static $http: ng.IHttpService;
    
        constructor($http: ng.IHttpService) {
            AngularEndpointsService.$http = $http;
        }
    
        static call<TView>(endpoint: IEndpoint, data) {
            var call = AngularEndpointsService.$http<TView>({
                method: endpoint.verb,
                url: endpoint.toString(),
                data: data
            });
        
            return call.then(response => response.data);
        }
    
        public Test = {
            Get: (hole?: string): Endpoints.Test.IGet => {
                var endpoint = new Endpoints.Test.Get(hole);
            
                var callHook = {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                }
            
                return _.extend(endpoint, callHook);
            },
        
            Get1: (id: string, hole?: string): Endpoints.Test.IGet1 => {
                var endpoint = new Endpoints.Test.Get1(id, hole);
            
                var callHook = {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                }
            
                return _.extend(endpoint, callHook);
            },
        
            GetSomething: (id: number, hole?: string, y?: Enums.DummyEnum): Endpoints.Test.IGetSomething => {
                var endpoint = new Endpoints.Test.GetSomething(id, hole, y);
            
                var callHook = {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                }
            
                return _.extend(endpoint, callHook);
            },
        
            GetSomethingElse: (id: number, y?: Interfaces.DummyClass, hole?: string): Endpoints.Test.IGetSomethingElse => {
                var endpoint = new Endpoints.Test.GetSomethingElse(id, y, hole);
            
                var callHook = {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                }
            
                return _.extend(endpoint, callHook);
            },
        
            Post: (hole?: string): Endpoints.Test.IPost => {
                var endpoint = new Endpoints.Test.Post(hole);
            
                var callHook = {
                    call<TView>(value: Interfaces.DummyClass) {
                        return AngularEndpointsService.call<TView>(this, value != null ? value : null);
                    }
                }
            
                return _.extend(endpoint, callHook);
            },
        
            Put: (id: number, hole?: string): Endpoints.Test.IPut => {
                var endpoint = new Endpoints.Test.Put(id, hole);
            
                var callHook = {
                    call<TView>(value: string) {
                        return AngularEndpointsService.call<TView>(this, value != null ? `"${value}"` : null);
                    }
                }
            
                return _.extend(endpoint, callHook);
            },
        
            Delete: (id: number, hole?: string): Endpoints.Test.IDelete => {
                var endpoint = new Endpoints.Test.Delete(id, hole);
            
                var callHook = {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    }
                }
            
                return _.extend(endpoint, callHook);
            }
        }
    }
}
