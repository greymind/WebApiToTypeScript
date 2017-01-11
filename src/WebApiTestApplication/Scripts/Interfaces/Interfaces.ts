namespace Interfaces {
    export interface IDummyClass {
        name: string;
        date: string;
        c: Interfaces.IAnotherClass;
    }

    export class DummyClass implements IDummyClass, Endpoints.IHaveQueryParams {
        name: string;
        date: string;
        c: Interfaces.AnotherClass;
    
        getQueryParams() {
            return this;
        }
    }

    export interface IAnotherClass {
        number: string | number;
        name: string;
        list: string[];
    }

    export class AnotherClass implements IAnotherClass, Endpoints.IHaveQueryParams {
        number: string | number;
        name: string;
        list: string[];
    
        getQueryParams() {
            return this;
        }
    }

    export interface IDerivedClassWithShadowedProperty extends IAnotherClass {
        number: number | string;
    }

    export class DerivedClassWithShadowedProperty extends AnotherClass implements IDerivedClassWithShadowedProperty, Endpoints.IHaveQueryParams {
        number: number | string;
    
        constructor() {
            super();
        }
    
        getQueryParams() {
            return this;
        }
    }

    export interface IDerivedClassWithAnotherShadowedProperty extends IDerivedClassWithShadowedProperty {
        number: number;
    }

    export class DerivedClassWithAnotherShadowedProperty extends DerivedClassWithShadowedProperty implements IDerivedClassWithAnotherShadowedProperty, Endpoints.IHaveQueryParams {
        number: number;
    
        constructor() {
            super();
        }
    
        getQueryParams() {
            return this;
        }
    }

    export interface IMegaClass extends IAnotherClass {
        something: number;
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
}
