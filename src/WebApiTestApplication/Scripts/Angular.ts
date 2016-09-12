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

        endpointsService.Test.Get({
            hole: "cap"
        })
            .call()
            .then(responsePrinter);

        endpointsService.Test.Get1({
            hole: "cap",
            id: "777"
        })
            .call()
            .then(responsePrinter);

        endpointsService.Test.GetSomething({
            hole: "cap",
            id: 7,
            y: Enums.DummyEnum.Bye
        })
            .call()
            .then(responsePrinter);

        endpointsService.Test.GetSomethingElse({
            hole: "cap",
            id: 3,
            y: testObject
        })
            .call()
            .then(responsePrinter);

        endpointsService.Test.Post({
            hole: "cap"
        })
            .call(null)
            .then(responsePrinter);

        endpointsService.Test.Post({
            hole: "cap"
        })
            .call(testObject)
            .then(responsePrinter);

        endpointsService.Test.Put({
            hole: "cap",
            id: 5
        })
            .call("b")
            .then(responsePrinter);

        endpointsService.Test.Delete({
            hole: "cap",
            id: 2
        })
            .call()
            .then(responsePrinter);
    }
}

angular.module("TestApp")
    .service("AngularEndpointsService", Endpoints.AngularEndpointsService);

angular.module("TestApp")
    .controller("TestController", TestController);