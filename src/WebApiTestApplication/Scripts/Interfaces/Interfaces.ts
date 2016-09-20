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

    export interface IChain1Generic1<T> {
        value?: T;
    }

    export class Chain1Generic1<T> implements IChain1Generic1<T>, Endpoints.IHaveQueryParams {
        value: T;

        getQueryParams() {
            return this;
        }
    }

    export interface IChain1Generic2<T1, T2> {
        value11?: T1;
        value12?: T2;
    }

    export class Chain1Generic2<T1, T2> implements IChain1Generic2<T1, T2>, Endpoints.IHaveQueryParams {
        value11: T1;
        value12: T2;

        getQueryParams() {
            return this;
        }
    }

    export interface IChain2Generic1<TValue> extends IChain1Generic2<TValue, number> {
        value2?: TValue;
    }

    export class Chain2Generic1<TValue> extends Chain1Generic2<TValue, number> implements IChain2Generic1<TValue>, Endpoints.IHaveQueryParams {
        value2: TValue;

        constructor() {
            super();
        }

        getQueryParams() {
            return this;
        }
    }

    export interface IChain3 extends IChain2Generic1<Interfaces.MegaClass> {
        value3?: any;
    }

    export class Chain3 extends Chain2Generic1<Interfaces.MegaClass> implements IChain3, Endpoints.IHaveQueryParams {
        value3: any;

        constructor() {
            super();
        }

        getQueryParams() {
            return this;
        }
    }
}