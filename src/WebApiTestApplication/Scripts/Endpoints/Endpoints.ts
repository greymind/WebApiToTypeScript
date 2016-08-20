namespace Endpoints {
    export module TestEndpoint {
        export class Get {
            constructor() {
            }

            toString(): string {
                //HttpGetAttribute
                return `/api/Test/Get`;
            }

        }

        export class Get1 {
            id: number
        
            constructor(id) {
                this.id = id;
            }

            toString(): string {
                //HttpGetAttribute
                return `/api/Test/Get1`;
            }

        }

        export class GetSomething {
            id: number
            y: number
        
            constructor(id) {
                this.id = id;
            }

            toString(): string {
                //HttpGetAttribute
                return `/api/Test/GetSomething`;
            }

        }

        export class Post {
            value: string
        
            constructor(value) {
                this.value = value;
            }

            toString(): string {
                //HttpPostAttribute
                return `/api/Test/Post`;
            }

        }

        export class Put {
            id: number
            value: string
        
            constructor(id, value) {
                this.id = id;
                this.value = value;
            }

            toString(): string {
                //HttpPutAttribute
                return `/api/Test/Put`;
            }

        }

        export class Delete {
            id: number
        
            constructor(id) {
                this.id = id;
            }

            toString(): string {
                //HttpDeleteAttribute
                return `/api/Test/Delete`;
            }

        }

    }

}
