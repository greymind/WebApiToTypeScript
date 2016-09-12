namespace Endpoints {
    export interface IEndpoint {
        _verb: string;
        toString(): string;
    }

    export interface IHaveQueryParams {
        getQueryParams(): Object
    }

    export namespace Test {
        export interface IGet {
            hole: string;
        }
    
        export interface IGetWithCall extends IGet, IEndpoint {
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class Get implements IGet, IEndpoint {
            _verb = 'GET';
            hole: string;
        
            constructor(args: IGet) {
                this.hole = args.hole;
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/GetAll`;
            }
        }
    
        export interface IGet1 {
            id: string;
            hole: string;
        }
    
        export interface IGet1WithCall extends IGet1, IEndpoint {
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class Get1 implements IGet1, IEndpoint {
            _verb = 'GET';
            id: string;
            hole: string;
        
            constructor(args: IGet1) {
                this.id = args.id;
                this.hole = args.hole;
            }
        
            private getQueryString = (): string => {
                var parameters: string[] = [];
            
                if (this.id != null) {
                    parameters.push(`id=${encodeURIComponent(this.id.toString())}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions` + this.getQueryString();
            }
        }
    
        export interface IGetSomething {
            id: number;
            hole: string;
            y?: Enums.DummyEnum;
        }
    
        export interface IGetSomethingWithCall extends IGetSomething, IEndpoint {
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class GetSomething implements IGetSomething, IEndpoint {
            _verb = 'GET';
            id: number;
            hole: string;
            y: Enums.DummyEnum;
        
            constructor(args: IGetSomething) {
                this.id = args.id;
                this.hole = args.hole;
                this.y = args.y;
            }
        
            private getQueryString = (): string => {
                var parameters: string[] = [];
            
                if (this.y != null) {
                    parameters.push(`y=${encodeURIComponent(this.y.toString())}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/getSomething/${this.id}/ha` + this.getQueryString();
            }
        }
    
        export interface IGetSomethingElse {
            id: number;
            y?: Interfaces.DummyClass;
            hole: string;
        }
    
        export interface IGetSomethingElseWithCall extends IGetSomethingElse, IEndpoint {
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class GetSomethingElse implements IGetSomethingElse, IEndpoint {
            _verb = 'GET';
            id: number;
            y: Interfaces.DummyClass;
            hole: string;
        
            constructor(args: IGetSomethingElse) {
                this.id = args.id;
                this.y = args.y;
                this.hole = args.hole;
            }
        
            private getQueryString = (): string => {
                var parameters: string[] = [];
            
                if (this.id != null) {
                    parameters.push(`id=${encodeURIComponent(this.id.toString())}`);
                }
            
                if (this.y != null) {
                    var yParams = this.y.getQueryParams();
                    Object.keys(yParams).forEach((key) => {
                        if (yParams[key] != null) {
                            parameters.push(`${key}=${encodeURIComponent(yParams[key].toString())}`);
                        }
                    });
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/GetSomethingElse` + this.getQueryString();
            }
        }
    
        export interface IPost {
            hole: string;
        }
    
        export interface IPostWithCall extends IPost, IEndpoint {
            call<TView>(value: Interfaces.DummyClass): ng.IPromise<TView>;
        }
    
        export class Post implements IPost, IEndpoint {
            _verb = 'POST';
            hole: string;
        
            constructor(args: IPost) {
                this.hole = args.hole;
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions`;
            }
        }
    
        export interface IPut {
            id: number;
            hole: string;
        }
    
        export interface IPutWithCall extends IPut, IEndpoint {
            call<TView>(value: string): ng.IPromise<TView>;
        }
    
        export class Put implements IPut, IEndpoint {
            _verb = 'PUT';
            id: number;
            hole: string;
        
            constructor(args: IPut) {
                this.id = args.id;
                this.hole = args.hole;
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/${this.id}`;
            }
        }
    
        export interface IDelete {
            id: number;
            hole: string;
        }
    
        export interface IDeleteWithCall extends IDelete, IEndpoint {
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class Delete implements IDelete, IEndpoint {
            _verb = 'DELETE';
            id: number;
            hole: string;
        
            constructor(args: IDelete) {
                this.id = args.id;
                this.hole = args.hole;
            }
        
            private getQueryString = (): string => {
                var parameters: string[] = [];
            
                if (this.id != null) {
                    parameters.push(`id=${encodeURIComponent(this.id.toString())}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions` + this.getQueryString();
            }
        }
    }
}
