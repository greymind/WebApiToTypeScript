namespace Enums {
    export enum DummyEnum {
        Hi = 1,
        Bye = 2,
        ValueWithMultipleUppercaseWords = 3,
    }

    export namespace DummyEnum {
        export function getDescription(enumValue: DummyEnum) {
            switch (enumValue) {
                case DummyEnum.Hi: return "Hi";
                case DummyEnum.Bye: return "Bye a lot";
                case DummyEnum.ValueWithMultipleUppercaseWords: return "Value With Multiple Uppercase Words";
            }
        }
    }
}
