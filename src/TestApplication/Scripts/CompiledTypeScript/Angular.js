//angular.module("TestApp", []);
//class TestController {
//    static $inject = ["$http", "AngularEndpointsService"];
//    constructor(
//        $http: ng.IHttpService,
//        endpointsService: Endpoints.AngularEndpointsService) {
//        function responsePrinter(data) {
//            console.log(data);
//        }
//        var dummyClass = new Interfaces.DummyClass();
//        dummyClass.name = "cappy";
//        dummyClass.date = new Date().toJSON();
//        var anotherClass = new Interfaces.AnotherClass();
//        anotherClass.name = "bappy";
//        anotherClass.number = 25;
//        anotherClass.list = ["balki", "monkey"];
//        dummyClass.c = anotherClass;
//        var megaClass: Interfaces.MegaClass = _.extend(new Interfaces.MegaClass(), anotherClass);
//        megaClass.something = 7;
//        endpointsService.Test.Get({
//                hole: "cap"
//            })
//            .call()
//            .then(responsePrinter);
//        endpointsService.Test.Get1({
//                hole: "cap",
//                id: "777"
//            })
//            .call()
//            .then(responsePrinter);
//        endpointsService.Test.GetSomething({
//                hole: "cap",
//                id: 7,
//                y: Enums.DummyEnum.Bye
//            })
//            .call()
//            .then(responsePrinter);
//        endpointsService.Test.GetSomethingElse({
//                hole: "cap",
//                id: 3,
//                y: dummyClass
//            })
//            .call()
//            .then(responsePrinter);
//        endpointsService.Test.Post({
//                hole: "cap"
//            })
//            .call(null)
//            .then(responsePrinter);
//        endpointsService.Test.Post({
//                hole: "cap"
//            })
//            .call(dummyClass)
//            .then(responsePrinter);
//        endpointsService.Test.Put({
//                hole: "cap",
//                id: 5
//            })
//            .call("b")
//            .then(responsePrinter);
//        endpointsService.Test.Delete({
//                hole: "cap",
//                id: 2
//            })
//            .call()
//            .then(responsePrinter);
//        endpointsService.Thingy.Get({
//                id: 1,
//                x: "blah",
//                c: megaClass
//            })
//            .call()
//            .then(responsePrinter);
//        endpointsService.Thingy.Post()
//            .call({
//                something: 7,
//                number: 1,
//                name: null,
//                list: null
//            })
//            .then(responsePrinter);
//    }
//}
//angular.module("TestApp")
//    .service("AngularEndpointsService", Endpoints.AngularEndpointsService);
//angular.module("TestApp")
//    .controller("TestController", TestController); 
//# sourceMappingURL=Angular.js.map