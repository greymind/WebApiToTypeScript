namespace Endpoints {
    export class AngularEndpointsService {
        static $inject = ['$http', '$q'];
        static endpointCache = {};
    
        constructor($http: ng.IHttpService, $q: ng.IQService) {
            this.Test.Get = (args: Endpoints.Test.IGet): Endpoints.Test.IGetWithCall => {
                var endpoint = new Endpoints.Test.Get(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string[]>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string[]>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.Get1 = (args: Endpoints.Test.IGet1): Endpoints.Test.IGet1WithCall => {
                var endpoint = new Endpoints.Test.Get1(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.GetSomething = (args: Endpoints.Test.IGetSomething): Endpoints.Test.IGetSomethingWithCall => {
                var endpoint = new Endpoints.Test.GetSomething(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.GetSomethingElse = (args: Endpoints.Test.IGetSomethingElse): Endpoints.Test.IGetSomethingElseWithCall => {
                var endpoint = new Endpoints.Test.GetSomethingElse(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.GetEnumerableString = (args: Endpoints.Test.IGetEnumerableString): Endpoints.Test.IGetEnumerableStringWithCall => {
                var endpoint = new Endpoints.Test.GetEnumerableString(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string[]>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string[]>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.GetIHttpActionResult = (args: Endpoints.Test.IGetIHttpActionResult): Endpoints.Test.IGetIHttpActionResultWithCall => {
                var endpoint = new Endpoints.Test.GetIHttpActionResult(args);
                return _.extendOwn(endpoint, {
                    call<TView>(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<TView>($http, this, null, httpConfig);
                    },
                
                    callCached<TView>(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<TView>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.GetVoidTask = (args: Endpoints.Test.IGetVoidTask): Endpoints.Test.IGetVoidTaskWithCall => {
                var endpoint = new Endpoints.Test.GetVoidTask(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<void>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<void>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.GetStringTask = (args: Endpoints.Test.IGetStringTask): Endpoints.Test.IGetStringTaskWithCall => {
                var endpoint = new Endpoints.Test.GetStringTask(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.GetEnumerableStringTask = (args: Endpoints.Test.IGetEnumerableStringTask): Endpoints.Test.IGetEnumerableStringTaskWithCall => {
                var endpoint = new Endpoints.Test.GetEnumerableStringTask(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string[]>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string[]>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Test.Post = (args: Endpoints.Test.IPost): Endpoints.Test.IPostWithCall => {
                var endpoint = new Endpoints.Test.Post(args);
                return _.extendOwn(endpoint, {
                    call(value: Interfaces.IDummyClass, httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
        
            this.Test.Post1 = (args: Endpoints.Test.IPost1): Endpoints.Test.IPost1WithCall => {
                var endpoint = new Endpoints.Test.Post1(args);
                return _.extendOwn(endpoint, {
                    call(value: Interfaces.IDerivedClassWithShadowedProperty, httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
        
            this.Test.Post2 = (args: Endpoints.Test.IPost2): Endpoints.Test.IPost2WithCall => {
                var endpoint = new Endpoints.Test.Post2(args);
                return _.extendOwn(endpoint, {
                    call(value: Interfaces.IDerivedClassWithAnotherShadowedProperty, httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
        
            this.Test.Put = (args: Endpoints.Test.IPut): Endpoints.Test.IPutWithCall => {
                var endpoint = new Endpoints.Test.Put(args);
                return _.extendOwn(endpoint, {
                    call(value: string, httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, value != null ? `"${value}"` : null, httpConfig);
                    },
                });
            };
        
            this.Test.Delete = (args: Endpoints.Test.IDelete): Endpoints.Test.IDeleteWithCall => {
                var endpoint = new Endpoints.Test.Delete(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                });
            };
        
            this.Thingy.GetAll = (args?: Endpoints.Thingy.IGetAll): Endpoints.Thingy.IGetAllWithCall => {
                var endpoint = new Endpoints.Thingy.GetAll(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Thingy.Get = (args: Endpoints.Thingy.IGet): Endpoints.Thingy.IGetWithCall => {
                var endpoint = new Endpoints.Thingy.Get(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Thingy.Getty = (args: Endpoints.Thingy.IGetty): Endpoints.Thingy.IGettyWithCall => {
                var endpoint = new Endpoints.Thingy.Getty(args);
                return _.extendOwn(endpoint, {
                    call(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, null, httpConfig);
                    },
                
                    callCached(httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.callCached<string>($http, $q, this, null, httpConfig);
                    }
                });
            };
        
            this.Thingy.Post = (args?: Endpoints.Thingy.IPost): Endpoints.Thingy.IPostWithCall => {
                var endpoint = new Endpoints.Thingy.Post(args);
                return _.extendOwn(endpoint, {
                    call(value: Interfaces.IMegaClass, httpConfig?: ng.IRequestShortcutConfig) {
                        return AngularEndpointsService.call<string>($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
        }
    
        static call<TView>(httpService: ng.IHttpService, endpoint: IEndpoint, data, httpConfig?: ng.IRequestShortcutConfig) {
            const config =  {
                method: endpoint._verb,
                url: endpoint.toString(),
                data: data
            }
        
            httpConfig && _.extend(config, httpConfig);
            
            const call = httpService<TView>(config);
            return call.then(response => response.data);
        }
    
        static callCached<TView>(httpService: ng.IHttpService, qService: ng.IQService, endpoint: IEndpoint, data, httpConfig?: ng.IRequestShortcutConfig) {
            var cacheKey = endpoint.toString();
        
            if (this.endpointCache[cacheKey] == null) {
                return this.call<TView>(httpService, endpoint, data, httpConfig).then(result => {
                    this.endpointCache[cacheKey] = result;
                    return this.endpointCache[cacheKey];
                });
            }
        
            const deferred = qService.defer();
            deferred.resolve(this.endpointCache[cacheKey]);
            return deferred.promise;
        }
    
        public Test: Endpoints.Test.ITestService;
        public Thingy: Endpoints.Thingy.IThingyService;
    }
}
