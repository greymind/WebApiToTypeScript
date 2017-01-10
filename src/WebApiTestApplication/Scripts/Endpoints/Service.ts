namespace Endpoints {
    export class AngularEndpointsService {
        static $inject = ['$http', '$q'];
        static $http: ng.IHttpService;
        static $q: ng.IQService;
        static endpointCache = {};
    
        constructor($http: ng.IHttpService, $q: ng.IQService) {
            AngularEndpointsService.$http = $http;
            AngularEndpointsService.$q = $q;
        }
    
        static call<TView>(endpoint: IEndpoint, data) {
            var call = AngularEndpointsService.$http<TView>({
                method: endpoint._verb,
                url: endpoint.toString(),
                data: data
            });
        
            return call.then(response => response.data);
        }
    
        static callCached<TView>(endpoint: IEndpoint, data) {
            var cacheKey = endpoint.toString();
        
            if (this.endpointCache[cacheKey] == null) {
                return this.call<TView>(endpoint, data).then(result => {
                    this.endpointCache[cacheKey] = result;
                    return this.endpointCache[cacheKey];
                });
            }
        
            const deferred = this.$q.defer();
            deferred.resolve(this.endpointCache[cacheKey]);
            return deferred.promise;
        }
    
        public Test: Endpoints.Test.ITestService = {
            Get: (args: Endpoints.Test.IGet): Endpoints.Test.IGetWithCall => {
                var endpoint = new Endpoints.Test.Get(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                
                    callCached<TView>() {
                        return AngularEndpointsService.callCached<TView>(this, null);
                    }
                });
            },
        
            Get1: (args: Endpoints.Test.IGet1): Endpoints.Test.IGet1WithCall => {
                var endpoint = new Endpoints.Test.Get1(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                
                    callCached<TView>() {
                        return AngularEndpointsService.callCached<TView>(this, null);
                    }
                });
            },
        
            GetSomething: (args: Endpoints.Test.IGetSomething): Endpoints.Test.IGetSomethingWithCall => {
                var endpoint = new Endpoints.Test.GetSomething(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                
                    callCached<TView>() {
                        return AngularEndpointsService.callCached<TView>(this, null);
                    }
                });
            },
        
            GetSomethingElse: (args: Endpoints.Test.IGetSomethingElse): Endpoints.Test.IGetSomethingElseWithCall => {
                var endpoint = new Endpoints.Test.GetSomethingElse(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                
                    callCached<TView>() {
                        return AngularEndpointsService.callCached<TView>(this, null);
                    }
                });
            },
        
            Post: (args: Endpoints.Test.IPost): Endpoints.Test.IPostWithCall => {
                var endpoint = new Endpoints.Test.Post(args);
                return _.extendOwn(endpoint, {
                    call<TView>(value: Interfaces.IDummyClass) {
                        return AngularEndpointsService.call<TView>(this, value != null ? value : null);
                    },
                });
            },
        
            Post1: (args: Endpoints.Test.IPost1): Endpoints.Test.IPost1WithCall => {
                var endpoint = new Endpoints.Test.Post1(args);
                return _.extendOwn(endpoint, {
                    call<TView>(value: Interfaces.IDerivedClassWithShadowedProperty) {
                        return AngularEndpointsService.call<TView>(this, value != null ? value : null);
                    },
                });
            },
        
            Post2: (args: Endpoints.Test.IPost2): Endpoints.Test.IPost2WithCall => {
                var endpoint = new Endpoints.Test.Post2(args);
                return _.extendOwn(endpoint, {
                    call<TView>(value: Interfaces.IDerivedClassWithAnotherShadowedProperty) {
                        return AngularEndpointsService.call<TView>(this, value != null ? value : null);
                    },
                });
            },
        
            Put: (args: Endpoints.Test.IPut): Endpoints.Test.IPutWithCall => {
                var endpoint = new Endpoints.Test.Put(args);
                return _.extendOwn(endpoint, {
                    call<TView>(value: string) {
                        return AngularEndpointsService.call<TView>(this, value != null ? `"${value}"` : null);
                    },
                });
            },
        
            Delete: (args: Endpoints.Test.IDelete): Endpoints.Test.IDeleteWithCall => {
                var endpoint = new Endpoints.Test.Delete(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                });
            }
        }
    
        public Thingy: Endpoints.Thingy.IThingyService = {
            GetAll: (args?: Endpoints.Thingy.IGetAll): Endpoints.Thingy.IGetAllWithCall => {
                var endpoint = new Endpoints.Thingy.GetAll(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                
                    callCached<TView>() {
                        return AngularEndpointsService.callCached<TView>(this, null);
                    }
                });
            },
        
            Get: (args: Endpoints.Thingy.IGet): Endpoints.Thingy.IGetWithCall => {
                var endpoint = new Endpoints.Thingy.Get(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                
                    callCached<TView>() {
                        return AngularEndpointsService.callCached<TView>(this, null);
                    }
                });
            },
        
            Getty: (args: Endpoints.Thingy.IGetty): Endpoints.Thingy.IGettyWithCall => {
                var endpoint = new Endpoints.Thingy.Getty(args);
                return _.extendOwn(endpoint, {
                    call<TView>() {
                        return AngularEndpointsService.call<TView>(this, null);
                    },
                
                    callCached<TView>() {
                        return AngularEndpointsService.callCached<TView>(this, null);
                    }
                });
            },
        
            Post: (args?: Endpoints.Thingy.IPost): Endpoints.Thingy.IPostWithCall => {
                var endpoint = new Endpoints.Thingy.Post(args);
                return _.extendOwn(endpoint, {
                    call<TView>(value: Interfaces.IMegaClass) {
                        return AngularEndpointsService.call<TView>(this, value != null ? value : null);
                    },
                });
            }
        }
    }
}
