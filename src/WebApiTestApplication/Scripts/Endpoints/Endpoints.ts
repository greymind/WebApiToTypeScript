namespace Endpoints {
    export module TestEndpoint {
        export class Get {
            toString(): string {
                return '/api/Test/GetAll';
            }
        }
    
        export class Get1 {
            id: number
        
            constructor(id: number) {
                this.id = id;
            }
        
            private getQueryString(): string {
                var parameters: string[]
                
                parameters.push(`id=${this.id}`);
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString(): string {
                return '/api/Test/Get' + this.getQueryString();;
            }
        }
    
        export class GetSomething {
            id: number
            y: number
        
            constructor(id: number) {
                this.id = id;
            }
        
            private getQueryString(): string {
                var parameters: string[]
                
                parameters.push(`id=${this.id}`);
            
                if (this.y != null) {
                    parameters.push(`y=${this.y}`);
                }
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString(): string {
                return '/api/Test/GetSomething' + this.getQueryString();;
            }
        }
    
        export class Post {
            value: string
        
            constructor(value: string) {
                this.value = value;
            }
        
            private getQueryString(): string {
                var parameters: string[]
                
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString(): string {
                return '/api/Test/Post' + this.getQueryString();;
            }
        }
    
        export class Put {
            id: number
            value: string
        
            constructor(id: number, value: string) {
                this.id = id;
                this.value = value;
            }
        
            private getQueryString(): string {
                var parameters: string[]
                
                parameters.push(`id=${this.id}`);
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString(): string {
                return '/api/Test/Put' + this.getQueryString();;
            }
        }
    
        export class Delete {
            id: number
        
            constructor(id: number) {
                this.id = id;
            }
        
            private getQueryString(): string {
                var parameters: string[]
                
                parameters.push(`id=${this.id}`);
            
                if (parameters.length > 0) {
                    return '?' + parameters.join('&');
                }
            
                return '';
            }
        
            toString(): string {
                return '/api/Test/Delete' + this.getQueryString();;
            }
        }
    }
}
