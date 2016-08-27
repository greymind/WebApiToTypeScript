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

new Endpoints.Test.Get("cap").call().then(responsePrinter);
new Endpoints.Test.Get1("77", "cap").call().then(responsePrinter);
new Endpoints.Test.GetSomething(7, "cap", 2).call().then(responsePrinter);
new Endpoints.Test.GetSomethingElse(3, testObject, "cap").call().then(responsePrinter);
new Endpoints.Test.Post("cap").call(testObject).then(responsePrinter);
new Endpoints.Test.Put(5, "cap").call("data").then(responsePrinter);
new Endpoints.Test.Delete(2, "cap").call().then(responsePrinter);