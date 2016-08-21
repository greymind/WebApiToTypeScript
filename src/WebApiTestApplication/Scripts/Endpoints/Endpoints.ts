namespace Endpoints {
    export namespace TestEndpoint {
        export class Get {
            toString = (): string => {
                return `/api/Test/actions`;
            }
        }
    
        export class Get1 {
            constructor(public id: number) {
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
                return `/api/Test/actions` + this.getQueryString();;
            }
        }
    
        export class GetSomething {
            constructor(public id: number, public y?: number) {
            }
        
            private getQueryString = (): string => {
                let parameters: string[] = []
                
                parameters.push(`id=${this.id}`);
            
                if (this.y != null) {
                    parameters.push(`y=${this.y}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString = (): string => {
                return `/api/Test/actions` + this.getQueryString();;
            }
        }
    
        export class GetSomethingElse {
            constructor(public id: number, public y: string) {
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
                return `/api/Test/actions` + this.getQueryString();;
            }
        }
    
        export class Post {
            constructor(public value: string) {
            }
        
            toString = (): string => {
                return `/api/Test/actions`;
            }
        }
    
        export class Put {
            constructor(public id: number, public value: string) {
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
                return `/api/Test/actions` + this.getQueryString();;
            }
        }
    
        export class Delete {
            constructor(public id: number) {
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
                return `/api/Test/actions` + this.getQueryString();;
            }
        }
    }
}
