function responsePrinter(response) {
    console.log(response.status, response.data);
}

var testObject = new Interfaces.DummyClass();
testObject.name = "cappy";
testObject.date = new Date().toJSON();
var c = new Interfaces.AnotherClass();
c.name = "bappy";
c.number = 25;
testObject.c = c;

new Endpoints.Test.Get({
    hole: "cap"
});

new Endpoints.Test.Get1({
    hole: "cap",
    id: "777"
});

new Endpoints.Test.GetSomething({
    hole: "cap",
    id: 7,
    y: Enums.DummyEnum.Bye
});

new Endpoints.Test.GetSomethingElse({
    hole: "cap",
    id: 3,
    y: testObject
});

new Endpoints.Test.Post({
    hole: "cap"
});

new Endpoints.Test.Post({
    hole: "cap"
});

new Endpoints.Test.Put({
    hole: "cap",
    id: 5
});

new Endpoints.Test.Delete({
    hole: "cap",
    id: 2
});