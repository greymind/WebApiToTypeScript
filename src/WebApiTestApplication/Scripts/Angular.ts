angular.module("TestApp", []);

class TestController {
    static $inject = ["$http", "AngularEndpointsService"];

    constructor(
        $http: ng.IHttpService,
        endpointsService: Endpoints.AngularEndpointsService) {
        function responsePrinter(data) {
            console.log(data);
        }

        var testObject = new Interfaces.DummyClass();
        testObject.Name = "cappy";
        testObject.Date = new Date().toJSON();

        var c = new Interfaces.AnotherClass();
        c.Name = "bappy";
        c.Number = 25;

        testObject.C = c;

        endpointsService.Test.Get("cap")
            .call()
            .then(responsePrinter);

        endpointsService.Test.Get1("77", "cap")
            .call()
            .then(responsePrinter);

        endpointsService.Test.GetSomething(7, "cap", 2)
            .call()
            .then(responsePrinter);

        endpointsService.Test.GetSomethingElse(3, testObject, "cap")
            .call()
            .then(responsePrinter);

        endpointsService.Test.Post("cap")
            .call(null)
            .then(responsePrinter);

        endpointsService.Test.Post("cap")
            .call(testObject)
            .then(responsePrinter);

        endpointsService.Test.Put(5, "cap")
            .call("b")
            .then(responsePrinter);

        endpointsService.Test.Delete(2, "cap")
            .call()
            .then(responsePrinter);
    }
}

angular.module("TestApp")
    .service("AngularEndpointsService", Endpoints.AngularEndpointsService);

angular.module("TestApp")
    .controller("TestController", TestController);