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

    export interface IChain1Generic<T> {
        value1?: T;
    }

    export class Chain1Generic<T> implements IChain1Generic<T>, Endpoints.IHaveQueryParams {
        value1: T;
    
        getQueryParams() {
            return this;
        }
    }

    export interface IChain2Generic<TValue> extends IChain1Generic<TValue> {
        value2?: TValue;
    }

    export class Chain2Generic<TValue> extends Chain1Generic<TValue> implements IChain2Generic<TValue>, Endpoints.IHaveQueryParams {
        value2: TValue;
    
        constructor() {
            super();
        }
    
        getQueryParams() {
            return this;
        }
    }

    export interface IChain3 extends IChain2Generic<Interfaces.MegaClass> {
        value3?: any;
    }

    export class Chain3 extends Chain2Generic<Interfaces.MegaClass> implements IChain3, Endpoints.IHaveQueryParams {
        value3: any;
    
        constructor() {
            super();
        }
    
        getQueryParams() {
            return this;
        }
    }
}
