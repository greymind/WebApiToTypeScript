namespace Endpoints {
    export namespace TestEndpoint {
        export class Get {
            verb: string = 'GET'
        
            constructor(public hole: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/GetAll`;
            }
        }
    
        export class Get1 {
            verb: string = 'GET'
        
            constructor(public id: number, public hole: string) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = []
                
                parameters.push(`id=${this.id}`);
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions` + this.getQueryString();;
            }
        }
    
        export class GetSomething {
            verb: string = 'GET'
        
            constructor(public hole: string, public id: number, public y?: number) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = []
                
            
                if (this.y != null) {
                    parameters.push(`y=${this.y}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/getSomething/${this.id}/ha` + this.getQueryString();;
            }
        }
    
        export class GetSomethingElse {
            verb: string = 'GET'
        
            constructor(public id: number, public y: string, public hole: string) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = []
                
                parameters.push(`id=${this.id}`);
                parameters.push(`y=${this.y}`);
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/GetSomethingElse` + this.getQueryString();;
            }
        }
    
        export class Post {
            verb: string = 'POST'
        
            constructor(public hole: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions`;
            }
        }
    
        export class Put {
            verb: string = 'PUT'
        
            constructor(public id: number, public hole: string) {
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions/${this.id}`;
            }
        }
    
        export class Delete {
            verb: string = 'DELETE'
        
            constructor(public id: number, public hole: string) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = []
                
                parameters.push(`id=${this.id}`);
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/${this.hole}/actions` + this.getQueryString();;
            }
        }
    }
}
