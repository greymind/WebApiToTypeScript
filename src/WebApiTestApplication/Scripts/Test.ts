function responsePrinter(response) {
    console.log(response.status, response.data);
}

var testObject = new Interfaces.DummyClass();
testObject.Name = "cappy";
testObject.Date = new Date().toJSON();
var c = new Interfaces.AnotherClass();
c.Name = "bappy";
c.Number = 25;
testObject.C = c;

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