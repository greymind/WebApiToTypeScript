namespace Endpoints {
    export interface IEndpoint {
        verb: string;
        toString(): string;
    }

    export interface IHaveQueryParams {
        getQueryParams(): Object
    }

    export namespace Test {
        export interface IGet extends IEndpoint {
            hole?: string;
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class Get implements IEndpoint {
            verb = 'GET';
        
            constructor(public hole?: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/GetAll`;
            }
        }
    
        export interface IGet1 extends IEndpoint {
            id: string;
            hole?: string;
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class Get1 implements IEndpoint {
            verb = 'GET';
        
            constructor(public id: string, public hole?: string) {
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
    
        export interface IGetSomething extends IEndpoint {
            id: number;
            hole?: string;
            y?: Enums.DummyEnum;
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class GetSomething implements IEndpoint {
            verb = 'GET';
        
            constructor(public id: number, public hole?: string, public y?: Enums.DummyEnum) {
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
    
        export interface IGetSomethingElse extends IEndpoint {
            id: number;
            y?: Interfaces.DummyClass;
            hole?: string;
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class GetSomethingElse implements IEndpoint {
            verb = 'GET';
        
            constructor(public id: number, public y?: Interfaces.DummyClass, public hole?: string) {
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
    
        export interface IPost extends IEndpoint {
            hole?: string;
            call<TView>(value: Interfaces.DummyClass): ng.IPromise<TView>;
        }
    
        export class Post implements IEndpoint {
            verb = 'POST';
        
            constructor(public hole?: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions`;
            }
        }
    
        export interface IPut extends IEndpoint {
            id: number;
            hole?: string;
            call<TView>(value: string): ng.IPromise<TView>;
        }
    
        export class Put implements IEndpoint {
            verb = 'PUT';
        
            constructor(public id: number, public hole?: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/${this.id}`;
            }
        }
    
        export interface IDelete extends IEndpoint {
            id: number;
            hole?: string;
            call<TView>(): ng.IPromise<TView>;
        }
    
        export class Delete implements IEndpoint {
            verb = 'DELETE';
        
            constructor(public id: number, public hole?: string) {
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
