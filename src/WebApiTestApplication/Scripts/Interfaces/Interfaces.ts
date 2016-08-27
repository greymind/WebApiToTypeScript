namespace Interfaces {
    export class DummyClass {
        Name: string;
        Date: string;
        C: AnotherClass;
    
        getQueryParams() {
            return this;
        }
    }

    export class AnotherClass {
        Number: number;
        Name: string;
    
        getQueryParams() {
            return this;
        }
    }
}
