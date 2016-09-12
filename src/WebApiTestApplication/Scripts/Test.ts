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

new Endpoints.Test.Get("cap");

new Endpoints.Test.Get1("77", "cap");

new Endpoints.Test.GetSomething(7, "cap", 2);

new Endpoints.Test.GetSomethingElse(3, testObject, "cap");

new Endpoints.Test.Post("cap");

new Endpoints.Test.Post("cap");

new Endpoints.Test.Put(5, "cap");

new Endpoints.Test.Delete(2, "cap");