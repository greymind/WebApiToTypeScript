namespace Endpoints {
    export interface IHaveQueryParams {
        getQueryParams(): Object
    }

    export namespace Test {
        export class Get {
            verb: string = 'GET';
        
            constructor(public hole?: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/GetAll`;
            }
        
            call = () => {
                let $http = angular.injector(['ng']).get('$http');
                return $http( {
                    method: 'GET',
                    url: `${this.toString()}`
                })
            }
        }
    
        export class Get1 {
            verb: string = 'GET';
        
            constructor(public id: string, public hole?: string) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = [];
            
                if (this.id != null) {
                    parameters.push(`id=${this.id}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions` + this.getQueryString();
            }
        
            call = () => {
                let $http = angular.injector(['ng']).get('$http');
                return $http( {
                    method: 'GET',
                    url: `${this.toString()}`
                })
            }
        }
    
        export class GetSomething {
            verb: string = 'GET';
        
            constructor(public id: number, public hole?: string, public y?: Enums.DummyEnum) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = [];
            
                if (this.y != null) {
                    parameters.push(`y=${this.y}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/getSomething/${this.id}/ha` + this.getQueryString();
            }
        
            call = () => {
                let $http = angular.injector(['ng']).get('$http');
                return $http( {
                    method: 'GET',
                    url: `${this.toString()}`
                })
            }
        }
    
        export class GetSomethingElse {
            verb: string = 'GET';
        
            constructor(public id: number, public y?: Interfaces.DummyClass, public hole?: string) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = [];
            
                if (this.id != null) {
                    parameters.push(`id=${this.id}`);
                }
            
                if (this.y != null) {
                    let yParams = this.y.getQueryParams();
                    Object.keys(yParams).forEach((key) => {
                        if (yParams[key] != null) {
                            parameters.push(`${key}=${yParams[key]}`);
                        }
                    })
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/GetSomethingElse` + this.getQueryString();
            }
        
            call = () => {
                let $http = angular.injector(['ng']).get('$http');
                return $http( {
                    method: 'GET',
                    url: `${this.toString()}`
                })
            }
        }
    
        export class Post {
            verb: string = 'POST';
        
            constructor(public hole?: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions`;
            }
        
            call = (value?: Interfaces.DummyClass) => {
                let $http = angular.injector(['ng']).get('$http');
                return $http( {
                    method: 'POST',
                    url: `${this.toString()}`,
                    data: value
                })
            }
        }
    
        export class Put {
            verb: string = 'PUT';
        
            constructor(public id: number, public hole?: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/${this.id}`;
            }
        
            call = (value?: string) => {
                let $http = angular.injector(['ng']).get('$http');
                return $http( {
                    method: 'PUT',
                    url: `${this.toString()}`,
                    data: `"${value}"`
                })
            }
        }
    
        export class Delete {
            verb: string = 'DELETE';
        
            constructor(public id: number, public hole?: string) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = [];
            
                if (this.id != null) {
                    parameters.push(`id=${this.id}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions` + this.getQueryString();
            }
        
            call = () => {
                let $http = angular.injector(['ng']).get('$http');
                return $http( {
                    method: 'DELETE',
                    url: `${this.toString()}`
                })
            }
        }
    }
}
