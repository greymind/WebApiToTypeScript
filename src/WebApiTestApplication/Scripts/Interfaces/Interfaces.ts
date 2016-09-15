namespace Interfaces {
    export interface IAnotherClass {
        number?: number;
        name?: string;
        list?: string[];
    }

    export class AnotherClass implements IAnotherClass, Endpoints.IHaveQueryParams {
        number: number;
        name: string;
        list: string[];
    
        getQueryParams() {
            return this;
        }
    }

    export interface IMegaClass extends IAnotherClass {
        something?: number;
    }

    export class MegaClass extends AnotherClass implements IMegaClass, Endpoints.IHaveQueryParams {
        something: number;
    
        constructor() {
            super();
        }
    
        getQueryParams() {
            return this;
        }
    }

    export interface IDummyClass {
        name?: string;
        date?: string;
        c?: Interfaces.IAnotherClass;
    }

    export class DummyClass implements IDummyClass, Endpoints.IHaveQueryParams {
        name: string;
        date: string;
        c: Interfaces.AnotherClass;
    
        getQueryParams() {
            return this;
        }
    }
}
