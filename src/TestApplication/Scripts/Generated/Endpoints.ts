namespace Endpoints {
    export interface IEndpoint {
        _verb: string;
        toString(): string;
    }

    function addParameter(parameters: string[], key: string, value: any) {
        if (value == null) {
            return;
        }
    
        if (_.isArray(value)) {
            var encodedItems = _.map(value, (item: any) => encodeURIComponent(item.toString()));
            _(encodedItems).each(item => parameters.push(`${key}=${item}`));
        }
    
        else if (_.isObject(value) && value.getQueryParams) {
            addParameter(parameters, key, value.getQueryParams());
        }
    
        else if (_.isObject(value)) {
            Object.keys(value).forEach((key) => { addParameter(parameters, key, value[key]); });
        }
        else {
            parameters.push(`${key}=${encodeURIComponent(value.toString())}`);
        }
    }

    export namespace Test {
        export interface ITestService {
            Get: (args?: IGet) => IGetWithCall
            Get1: (args?: IGet1) => IGet1WithCall
            GetSomething: (args?: IGetSomething) => IGetSomethingWithCall
            GetSomethingElse: (args?: IGetSomethingElse) => IGetSomethingElseWithCall
            GetEnumerableString: (args?: IGetEnumerableString) => IGetEnumerableStringWithCall
            GetIHttpActionResult: (args?: IGetIHttpActionResult) => IGetIHttpActionResultWithCall
            GetVoidTask: (args?: IGetVoidTask) => IGetVoidTaskWithCall
            GetStringTask: (args?: IGetStringTask) => IGetStringTaskWithCall
            GetEnumerableStringTask: (args?: IGetEnumerableStringTask) => IGetEnumerableStringTaskWithCall
            Post: (args?: IPost) => IPostWithCall
            Post1: (args?: IPost1) => IPost1WithCall
            Post2: (args?: IPost2) => IPost2WithCall
            Put: (args?: IPut) => IPutWithCall
            Delete: (args?: IDelete) => IDeleteWithCall
        }
    
        export interface IGet {
            hole: string;
        }
    
        export interface IGetEndpoint extends IGet, IEndpoint {
        }
    
        export interface IGetCtor {
            new(args?: IGet): IGetEndpoint
        }
    
        export interface IGetWithCall extends IGet, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string[]>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string[]>;
        }
    
        export var Get : IGetCtor = <any>(function(args?: IGet) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
    
        Get.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/GetAll`;
        }
    
        export interface IGet1 {
            id: string;
            hole: string;
        }
    
        export interface IGet1Endpoint extends IGet1, IEndpoint {
        }
    
        export interface IGet1Ctor {
            new(args?: IGet1): IGet1Endpoint
        }
    
        export interface IGet1WithCall extends IGet1, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Get1 : IGet1Ctor = <any>(function(args?: IGet1) {
            this._verb = 'GET';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
        });
    
        Get1.prototype.getQueryString = function(): string {
            var parameters: string[] = [];
            addParameter(parameters, 'id', this.id);
        
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
        
            return '';
        }
    
        Get1.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions` + this.getQueryString();
        }
    
        export interface IGetSomething {
            hole: string;
            id: number;
            y?: Enums.DummyEnum;
        }
    
        export interface IGetSomethingEndpoint extends IGetSomething, IEndpoint {
        }
    
        export interface IGetSomethingCtor {
            new(args?: IGetSomething): IGetSomethingEndpoint
        }
    
        export interface IGetSomethingWithCall extends IGetSomething, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var GetSomething : IGetSomethingCtor = <any>(function(args?: IGetSomething) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
            this.id = args != null ? args.id : null;
            this.y = args != null ? args.y : null;
        });
    
        GetSomething.prototype.getQueryString = function(): string {
            var parameters: string[] = [];
            addParameter(parameters, 'y', this.y);
        
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
        
            return '';
        }
    
        GetSomething.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/getSomething/${this.id}/ha` + this.getQueryString();
        }
    
        export interface IGetSomethingElse {
            id: number;
            hole: string;
            y?: Interfaces.IDummyClass;
        }
    
        export interface IGetSomethingElseEndpoint extends IGetSomethingElse, IEndpoint {
        }
    
        export interface IGetSomethingElseCtor {
            new(args?: IGetSomethingElse): IGetSomethingElseEndpoint
        }
    
        export interface IGetSomethingElseWithCall extends IGetSomethingElse, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var GetSomethingElse : IGetSomethingElseCtor = <any>(function(args?: IGetSomethingElse) {
            this._verb = 'GET';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
            this.y = args != null ? args.y : null;
        });
    
        GetSomethingElse.prototype.getQueryString = function(): string {
            var parameters: string[] = [];
            addParameter(parameters, 'id', this.id);
            addParameter(parameters, 'y', this.y);
        
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
        
            return '';
        }
    
        GetSomethingElse.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/GetSomethingElse` + this.getQueryString();
        }
    
        export interface IGetEnumerableString {
            hole: string;
        }
    
        export interface IGetEnumerableStringEndpoint extends IGetEnumerableString, IEndpoint {
        }
    
        export interface IGetEnumerableStringCtor {
            new(args?: IGetEnumerableString): IGetEnumerableStringEndpoint
        }
    
        export interface IGetEnumerableStringWithCall extends IGetEnumerableString, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string[]>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string[]>;
        }
    
        export var GetEnumerableString : IGetEnumerableStringCtor = <any>(function(args?: IGetEnumerableString) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
    
        GetEnumerableString.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/GetEnumerableString`;
        }
    
        export interface IGetIHttpActionResult {
            hole: string;
        }
    
        export interface IGetIHttpActionResultEndpoint extends IGetIHttpActionResult, IEndpoint {
        }
    
        export interface IGetIHttpActionResultCtor {
            new(args?: IGetIHttpActionResult): IGetIHttpActionResultEndpoint
        }
    
        export interface IGetIHttpActionResultWithCall extends IGetIHttpActionResult, IEndpoint {
            call<TView>(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<TView>;
            callCached<TView>(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<TView>;
        }
    
        export var GetIHttpActionResult : IGetIHttpActionResultCtor = <any>(function(args?: IGetIHttpActionResult) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
    
        GetIHttpActionResult.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/GetIHttpActionResult`;
        }
    
        export interface IGetVoidTask {
            hole: string;
        }
    
        export interface IGetVoidTaskEndpoint extends IGetVoidTask, IEndpoint {
        }
    
        export interface IGetVoidTaskCtor {
            new(args?: IGetVoidTask): IGetVoidTaskEndpoint
        }
    
        export interface IGetVoidTaskWithCall extends IGetVoidTask, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<void>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<void>;
        }
    
        export var GetVoidTask : IGetVoidTaskCtor = <any>(function(args?: IGetVoidTask) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
    
        GetVoidTask.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/GetVoidTask`;
        }
    
        export interface IGetStringTask {
            hole: string;
        }
    
        export interface IGetStringTaskEndpoint extends IGetStringTask, IEndpoint {
        }
    
        export interface IGetStringTaskCtor {
            new(args?: IGetStringTask): IGetStringTaskEndpoint
        }
    
        export interface IGetStringTaskWithCall extends IGetStringTask, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var GetStringTask : IGetStringTaskCtor = <any>(function(args?: IGetStringTask) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
    
        GetStringTask.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/GetStringTask`;
        }
    
        export interface IGetEnumerableStringTask {
            hole: string;
        }
    
        export interface IGetEnumerableStringTaskEndpoint extends IGetEnumerableStringTask, IEndpoint {
        }
    
        export interface IGetEnumerableStringTaskCtor {
            new(args?: IGetEnumerableStringTask): IGetEnumerableStringTaskEndpoint
        }
    
        export interface IGetEnumerableStringTaskWithCall extends IGetEnumerableStringTask, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string[]>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string[]>;
        }
    
        export var GetEnumerableStringTask : IGetEnumerableStringTaskCtor = <any>(function(args?: IGetEnumerableStringTask) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
    
        GetEnumerableStringTask.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/GetEnumerableStringTask`;
        }
    
        export interface IPost {
            hole: string;
        }
    
        export interface IPostEndpoint extends IPost, IEndpoint {
        }
    
        export interface IPostCtor {
            new(args?: IPost): IPostEndpoint
        }
    
        export interface IPostWithCall extends IPost, IEndpoint {
            call(value: Interfaces.IDummyClass, httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Post : IPostCtor = <any>(function(args?: IPost) {
            this._verb = 'POST';
            this.hole = args != null ? args.hole : null;
        });
    
        Post.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions`;
        }
    
        export interface IPost1 {
            hole: string;
        }
    
        export interface IPost1Endpoint extends IPost1, IEndpoint {
        }
    
        export interface IPost1Ctor {
            new(args?: IPost1): IPost1Endpoint
        }
    
        export interface IPost1WithCall extends IPost1, IEndpoint {
            call(value: Interfaces.IDerivedClassWithShadowedProperty, httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Post1 : IPost1Ctor = <any>(function(args?: IPost1) {
            this._verb = 'POST';
            this.hole = args != null ? args.hole : null;
        });
    
        Post1.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/derived`;
        }
    
        export interface IPost2 {
            hole: string;
        }
    
        export interface IPost2Endpoint extends IPost2, IEndpoint {
        }
    
        export interface IPost2Ctor {
            new(args?: IPost2): IPost2Endpoint
        }
    
        export interface IPost2WithCall extends IPost2, IEndpoint {
            call(value: Interfaces.IDerivedClassWithAnotherShadowedProperty, httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Post2 : IPost2Ctor = <any>(function(args?: IPost2) {
            this._verb = 'POST';
            this.hole = args != null ? args.hole : null;
        });
    
        Post2.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/derivedAgain`;
        }
    
        export interface IPut {
            id: number;
            hole: string;
        }
    
        export interface IPutEndpoint extends IPut, IEndpoint {
        }
    
        export interface IPutCtor {
            new(args?: IPut): IPutEndpoint
        }
    
        export interface IPutWithCall extends IPut, IEndpoint {
            call(value: string, httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Put : IPutCtor = <any>(function(args?: IPut) {
            this._verb = 'PUT';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
        });
    
        Put.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions/${this.id}`;
        }
    
        export interface IDelete {
            id: number;
            hole: string;
        }
    
        export interface IDeleteEndpoint extends IDelete, IEndpoint {
        }
    
        export interface IDeleteCtor {
            new(args?: IDelete): IDeleteEndpoint
        }
    
        export interface IDeleteWithCall extends IDelete, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Delete : IDeleteCtor = <any>(function(args?: IDelete) {
            this._verb = 'DELETE';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
        });
    
        Delete.prototype.getQueryString = function(): string {
            var parameters: string[] = [];
            addParameter(parameters, 'id', this.id);
        
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
        
            return '';
        }
    
        Delete.prototype.toString = function(): string {
            return `/api/Test/${this.hole}/actions` + this.getQueryString();
        }
    }

    export namespace Thingy {
        export interface IThingyService {
            GetAll: (args?: IGetAll) => IGetAllWithCall
            Get: (args?: IGet) => IGetWithCall
            Getty: (args?: IGetty) => IGettyWithCall
            Post: (args?: IPost) => IPostWithCall
        }
    
        export interface IGetAll {
        }
    
        export interface IGetAllEndpoint extends IGetAll, IEndpoint {
        }
    
        export interface IGetAllCtor {
            new(args?: IGetAll): IGetAllEndpoint
        }
    
        export interface IGetAllWithCall extends IGetAll, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var GetAll : IGetAllCtor = <any>(function(args?: IGetAll) {
            this._verb = 'GET';
        });
    
        GetAll.prototype.toString = function(): string {
            return `/api/thingy`;
        }
    
        export interface IGet {
            id: number;
            x?: string;
            c?: Interfaces.IMegaClass;
        }
    
        export interface IGetEndpoint extends IGet, IEndpoint {
        }
    
        export interface IGetCtor {
            new(args?: IGet): IGetEndpoint
        }
    
        export interface IGetWithCall extends IGet, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Get : IGetCtor = <any>(function(args?: IGet) {
            this._verb = 'GET';
            this.id = args != null ? args.id : null;
            this.x = args != null ? args.x : null;
            this.c = args != null ? args.c : null;
        });
    
        Get.prototype.getQueryString = function(): string {
            var parameters: string[] = [];
            addParameter(parameters, 'x', this.x);
            addParameter(parameters, 'c', this.c);
        
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
        
            return '';
        }
    
        Get.prototype.toString = function(): string {
            return `/api/thingy/${this.id}` + this.getQueryString();
        }
    
        export interface IGetty {
            y: number;
            x?: string;
        }
    
        export interface IGettyEndpoint extends IGetty, IEndpoint {
        }
    
        export interface IGettyCtor {
            new(args?: IGetty): IGettyEndpoint
        }
    
        export interface IGettyWithCall extends IGetty, IEndpoint {
            call(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
            callCached(httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Getty : IGettyCtor = <any>(function(args?: IGetty) {
            this._verb = 'GET';
            this.y = args != null ? args.y : null;
            this.x = args != null ? args.x : null;
        });
    
        Getty.prototype.getQueryString = function(): string {
            var parameters: string[] = [];
            addParameter(parameters, 'x', this.x);
            addParameter(parameters, 'y', this.y);
        
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
        
            return '';
        }
    
        Getty.prototype.toString = function(): string {
            return `/api/thingy` + this.getQueryString();
        }
    
        export interface IPost {
        }
    
        export interface IPostEndpoint extends IPost, IEndpoint {
        }
    
        export interface IPostCtor {
            new(args?: IPost): IPostEndpoint
        }
    
        export interface IPostWithCall extends IPost, IEndpoint {
            call(value: Interfaces.IMegaClass, httpConfig?: ng.IRequestShortcutConfig): ng.IPromise<string>;
        }
    
        export var Post : IPostCtor = <any>(function(args?: IPost) {
            this._verb = 'POST';
        });
    
        Post.prototype.toString = function(): string {
            return `/api/thingy`;
        }
    }
}
