angular.module("TestApp", []);

class TestController {
    static $inject = ["$http"];

    constructor($http: ng.IHttpService) {
    }
}

angular.module("TestApp")
    .controller("TestController", TestController);