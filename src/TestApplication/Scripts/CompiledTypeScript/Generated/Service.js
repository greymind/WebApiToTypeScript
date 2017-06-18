var Endpoints;
(function (Endpoints) {
    var AngularEndpointsService = (function () {
        function AngularEndpointsService($http, $q) {
            this.Test = {};
            this.Thingy = {};
            this.Test.Get = function (args) {
                var endpoint = new Endpoints.Test.Get(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.Get1 = function (args) {
                var endpoint = new Endpoints.Test.Get1(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.GetSomething = function (args) {
                var endpoint = new Endpoints.Test.GetSomething(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.GetSomethingElse = function (args) {
                var endpoint = new Endpoints.Test.GetSomethingElse(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.GetEnumerableString = function (args) {
                var endpoint = new Endpoints.Test.GetEnumerableString(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.GetIHttpActionResult = function (args) {
                var endpoint = new Endpoints.Test.GetIHttpActionResult(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.GetVoidTask = function (args) {
                var endpoint = new Endpoints.Test.GetVoidTask(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.GetStringTask = function (args) {
                var endpoint = new Endpoints.Test.GetStringTask(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.GetEnumerableStringTask = function (args) {
                var endpoint = new Endpoints.Test.GetEnumerableStringTask(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Test.Post = function (args) {
                var endpoint = new Endpoints.Test.Post(args);
                return _.extendOwn(endpoint, {
                    call: function (value, httpConfig) {
                        return AngularEndpointsService.call($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
            this.Test.Post1 = function (args) {
                var endpoint = new Endpoints.Test.Post1(args);
                return _.extendOwn(endpoint, {
                    call: function (value, httpConfig) {
                        return AngularEndpointsService.call($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
            this.Test.Post2 = function (args) {
                var endpoint = new Endpoints.Test.Post2(args);
                return _.extendOwn(endpoint, {
                    call: function (value, httpConfig) {
                        return AngularEndpointsService.call($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
            this.Test.Put = function (args) {
                var endpoint = new Endpoints.Test.Put(args);
                return _.extendOwn(endpoint, {
                    call: function (value, httpConfig) {
                        return AngularEndpointsService.call($http, this, value != null ? "\"" + value + "\"" : null, httpConfig);
                    },
                });
            };
            this.Test.Delete = function (args) {
                var endpoint = new Endpoints.Test.Delete(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                });
            };
            this.Thingy.GetAll = function (args) {
                var endpoint = new Endpoints.Thingy.GetAll(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Thingy.Get = function (args) {
                var endpoint = new Endpoints.Thingy.Get(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Thingy.Getty = function (args) {
                var endpoint = new Endpoints.Thingy.Getty(args);
                return _.extendOwn(endpoint, {
                    call: function (httpConfig) {
                        return AngularEndpointsService.call($http, this, null, httpConfig);
                    },
                    callCached: function (httpConfig) {
                        return AngularEndpointsService.callCached($http, $q, this, null, httpConfig);
                    }
                });
            };
            this.Thingy.Post = function (args) {
                var endpoint = new Endpoints.Thingy.Post(args);
                return _.extendOwn(endpoint, {
                    call: function (value, httpConfig) {
                        return AngularEndpointsService.call($http, this, value != null ? value : null, httpConfig);
                    },
                });
            };
        }
        AngularEndpointsService.call = function (httpService, endpoint, data, httpConfig) {
            var config = {
                method: endpoint._verb,
                url: endpoint.toString(),
                data: data
            };
            httpConfig && _.extend(config, httpConfig);
            var call = httpService(config);
            return call.then(function (response) { return response.data; });
        };
        AngularEndpointsService.callCached = function (httpService, qService, endpoint, data, httpConfig) {
            var _this = this;
            var cacheKey = endpoint.toString();
            if (this.endpointCache[cacheKey] == null) {
                return this.call(httpService, endpoint, data, httpConfig).then(function (result) {
                    _this.endpointCache[cacheKey] = result;
                    return _this.endpointCache[cacheKey];
                });
            }
            var deferred = qService.defer();
            deferred.resolve(this.endpointCache[cacheKey]);
            return deferred.promise;
        };
        return AngularEndpointsService;
    }());
    AngularEndpointsService.$inject = ['$http', '$q'];
    AngularEndpointsService.endpointCache = {};
    Endpoints.AngularEndpointsService = AngularEndpointsService;
})(Endpoints || (Endpoints = {}));
//# sourceMappingURL=Service.js.map