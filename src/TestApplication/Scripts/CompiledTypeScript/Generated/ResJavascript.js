var Resources;
(function (Resources) {
    Resources.ResJavascript = {
        TestWithNamedParameter: function (first, second) {
            return "This is the first " + first + " and this is the " + second + ".";
        },
        TestWithNamedParameterWithSubstring: function (first, second, firstsecond, secondfirst) {
            return "This is the first " + first + " and this is the " + second + " and this is " + firstsecond + " with " + secondfirst + ".";
        },
        TestWithSameParameterUsedTwice: function (slot0) {
            return "The parameter zero " + slot0 + " has been used twice here " + slot0 + ".";
        },
        TestWithIntegerParameterSubstring: function (slot0, slot1, slot10, slot01) {
            return "Substring test for slots " + slot0 + " and then " + slot1 + " and then " + slot10 + " and lastly " + slot01 + ".";
        },
    };
})(Resources || (Resources = {}));
//# sourceMappingURL=ResJavascript.js.map