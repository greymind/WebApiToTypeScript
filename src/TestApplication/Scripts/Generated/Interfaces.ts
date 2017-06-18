namespace Interfaces {
    export interface IDummyClass {
        name: string;
        date: string;
        c: Interfaces.IAnotherClass;
    }

    export class DummyClass implements IDummyClass {
        name: string;
        date: string;
        c: Interfaces.AnotherClass;
    }

    export interface IAnotherClass {
        number: string | number;
        name: string;
        list: string[];
    }

    export class AnotherClass implements IAnotherClass {
        number: string | number;
        name: string;
        list: string[];
    }

    export interface IDerivedClassWithShadowedProperty extends IAnotherClass {
        number: number | string;
    }

    export class DerivedClassWithShadowedProperty extends AnotherClass implements IDerivedClassWithShadowedProperty {
        number: number | string;
    
        constructor() {
            super();
        }
    }

    export interface IDerivedClassWithAnotherShadowedProperty extends IDerivedClassWithShadowedProperty {
        number: number;
    }

    export class DerivedClassWithAnotherShadowedProperty extends DerivedClassWithShadowedProperty implements IDerivedClassWithAnotherShadowedProperty {
        number: number;
    
        constructor() {
            super();
        }
    }

    export interface IMegaClass extends IAnotherClass {
        something: number;
    }

    export class MegaClass extends AnotherClass implements IMegaClass {
        something: number;
    
        constructor() {
            super();
        }
    }

    export interface IChain1Generic1<T> {
        value: T;
    }

    export class Chain1Generic1<T> implements IChain1Generic1<T> {
        value: T;
    }

    export interface IChain1Generic2<T1, T2> {
        value11: T1;
        value12: T2;
    }

    export class Chain1Generic2<T1, T2> implements IChain1Generic2<T1, T2> {
        value11: T1;
        value12: T2;
    }

    export interface IChain2Generic1<TValue> extends IChain1Generic2<TValue, number> {
        value2: TValue;
    }

    export class Chain2Generic1<TValue> extends Chain1Generic2<TValue, number> implements IChain2Generic1<TValue> {
        value2: TValue;
    
        constructor() {
            super();
        }
    }

    export interface IChain3 extends IChain2Generic1<Interfaces.MegaClass> {
        value3: any;
    }

    export class Chain3 extends Chain2Generic1<Interfaces.MegaClass> implements IChain3 {
        value3: any;
    
        constructor() {
            super();
        }
    }
}
