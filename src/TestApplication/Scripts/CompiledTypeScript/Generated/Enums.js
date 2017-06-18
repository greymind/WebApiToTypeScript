var Enums;
(function (Enums) {
    var DummyEnum;
    (function (DummyEnum) {
        DummyEnum[DummyEnum["Hi"] = 1] = "Hi";
        DummyEnum[DummyEnum["Bye"] = 2] = "Bye";
        DummyEnum[DummyEnum["ValueWithMultipleUppercaseWords"] = 3] = "ValueWithMultipleUppercaseWords";
    })(DummyEnum = Enums.DummyEnum || (Enums.DummyEnum = {}));
    (function (DummyEnum) {
        function getDescription(enumValue) {
            switch (enumValue) {
                case DummyEnum.Hi: return "Hi";
                case DummyEnum.Bye: return "Bye a lot";
                case DummyEnum.ValueWithMultipleUppercaseWords: return "Value With Multiple Uppercase Words";
            }
        }
        DummyEnum.getDescription = getDescription;
    })(DummyEnum = Enums.DummyEnum || (Enums.DummyEnum = {}));
    var MyEnum;
    (function (MyEnum) {
        MyEnum[MyEnum["Ok"] = 1] = "Ok";
        MyEnum[MyEnum["No"] = 2] = "No";
    })(MyEnum = Enums.MyEnum || (Enums.MyEnum = {}));
    (function (MyEnum) {
        function getDescription(enumValue) {
            switch (enumValue) {
                case MyEnum.Ok: return "Ok";
                case MyEnum.No: return "No";
            }
        }
        MyEnum.getDescription = getDescription;
    })(MyEnum = Enums.MyEnum || (Enums.MyEnum = {}));
})(Enums || (Enums = {}));
//# sourceMappingURL=Enums.js.map