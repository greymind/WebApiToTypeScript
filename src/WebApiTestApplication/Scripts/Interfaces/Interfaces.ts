namespace Interfaces {
    export class AnotherClass {
        Number: number;
        Name: string;
        List: string[];
    
        constructor() {
        }
    
        getQueryParams() {
            return this;
        }
    }

    export class DummyClass {
        Name: string;
        Date: string;
        C: Interfaces.AnotherClass;
    
        constructor() {
        }
    
        getQueryParams() {
            return this;
        }
    }
}
