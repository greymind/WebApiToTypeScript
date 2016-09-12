namespace Interfaces {
    export class AnotherClass {
        constructor(public Number?: number, public Name?: string) {
        }
    
        getQueryParams() {
            return this;
        }
    }

    export class DummyClass {
        constructor(public Name?: string, public Date?: string, public C?: AnotherClass) {
        }
    
        getQueryParams() {
            return this;
        }
    }
}
