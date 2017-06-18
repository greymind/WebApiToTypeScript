var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var Interfaces;
(function (Interfaces) {
    var DummyClass = (function () {
        function DummyClass() {
        }
        return DummyClass;
    }());
    Interfaces.DummyClass = DummyClass;
    var AnotherClass = (function () {
        function AnotherClass() {
        }
        return AnotherClass;
    }());
    Interfaces.AnotherClass = AnotherClass;
    var DerivedClassWithShadowedProperty = (function (_super) {
        __extends(DerivedClassWithShadowedProperty, _super);
        function DerivedClassWithShadowedProperty() {
            return _super.call(this) || this;
        }
        return DerivedClassWithShadowedProperty;
    }(AnotherClass));
    Interfaces.DerivedClassWithShadowedProperty = DerivedClassWithShadowedProperty;
    var DerivedClassWithAnotherShadowedProperty = (function (_super) {
        __extends(DerivedClassWithAnotherShadowedProperty, _super);
        function DerivedClassWithAnotherShadowedProperty() {
            return _super.call(this) || this;
        }
        return DerivedClassWithAnotherShadowedProperty;
    }(DerivedClassWithShadowedProperty));
    Interfaces.DerivedClassWithAnotherShadowedProperty = DerivedClassWithAnotherShadowedProperty;
    var MegaClass = (function (_super) {
        __extends(MegaClass, _super);
        function MegaClass() {
            return _super.call(this) || this;
        }
        return MegaClass;
    }(AnotherClass));
    Interfaces.MegaClass = MegaClass;
    var Chain1Generic1 = (function () {
        function Chain1Generic1() {
        }
        return Chain1Generic1;
    }());
    Interfaces.Chain1Generic1 = Chain1Generic1;
    var Chain1Generic2 = (function () {
        function Chain1Generic2() {
        }
        return Chain1Generic2;
    }());
    Interfaces.Chain1Generic2 = Chain1Generic2;
    var Chain2Generic1 = (function (_super) {
        __extends(Chain2Generic1, _super);
        function Chain2Generic1() {
            return _super.call(this) || this;
        }
        return Chain2Generic1;
    }(Chain1Generic2));
    Interfaces.Chain2Generic1 = Chain2Generic1;
    var Chain3 = (function (_super) {
        __extends(Chain3, _super);
        function Chain3() {
            return _super.call(this) || this;
        }
        return Chain3;
    }(Chain2Generic1));
    Interfaces.Chain3 = Chain3;
})(Interfaces || (Interfaces = {}));
//# sourceMappingURL=Interfaces.js.map