namespace Resources {
    export interface IResJavascript {
        TestWithNamedParameter : (first: string, second: string) => string;
        TestWithNamedParameterWithSubstring : (first: string, second: string, firstsecond: string, secondfirst: string) => string;
        TestWithSameParameterUsedTwice : (slot0: string) => string;
        TestWithIntegerParameterSubstring : (slot0: string, slot1: string, slot10: string, slot01: string) => string;
    }

    export var ResJavascript : IResJavascript =  {
        TestWithNamedParameter : function(first: string, second: string) {
            return `This is the first ${first} and this is the ${second}.`;
        },
    
        TestWithNamedParameterWithSubstring : function(first: string, second: string, firstsecond: string, secondfirst: string) {
            return `This is the first ${first} and this is the ${second} and this is ${firstsecond} with ${secondfirst}.`;
        },
    
        TestWithSameParameterUsedTwice : function(slot0: string) {
            return `The parameter zero ${slot0} has been used twice here ${slot0}.`;
        },
    
        TestWithIntegerParameterSubstring : function(slot0: string, slot1: string, slot10: string, slot01: string) {
            return `Substring test for slots ${slot0} and then ${slot1} and then ${slot10} and lastly ${slot01}.`;
        },
    }
}
