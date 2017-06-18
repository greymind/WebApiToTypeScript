var Endpoints;
(function (Endpoints) {
    function addParameter(parameters, key, value) {
        if (value == null) {
            return;
        }
        if (_.isArray(value)) {
            var encodedItems = _.map(value, function (item) { return encodeURIComponent(item.toString()); });
            _(encodedItems).each(function (item) { return parameters.push(key + "=" + item); });
        }
        else if (_.isObject(value) && value.getQueryParams) {
            addParameter(parameters, key, value.getQueryParams());
        }
        else if (_.isObject(value)) {
            Object.keys(value).forEach(function (key) { addParameter(parameters, key, value[key]); });
        }
        else {
            parameters.push(key + "=" + encodeURIComponent(value.toString()));
        }
    }
    var Test;
    (function (Test) {
        Test.Get = (function (args) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
        Test.Get.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/GetAll";
        };
        Test.Get1 = (function (args) {
            this._verb = 'GET';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
        });
        Test.Get1.prototype.getQueryString = function () {
            var parameters = [];
            addParameter(parameters, 'id', this.id);
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
            return '';
        };
        Test.Get1.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions" + this.getQueryString();
        };
        Test.GetSomething = (function (args) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
            this.id = args != null ? args.id : null;
            this.y = args != null ? args.y : null;
        });
        Test.GetSomething.prototype.getQueryString = function () {
            var parameters = [];
            addParameter(parameters, 'y', this.y);
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
            return '';
        };
        Test.GetSomething.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/getSomething/" + this.id + "/ha" + this.getQueryString();
        };
        Test.GetSomethingElse = (function (args) {
            this._verb = 'GET';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
            this.y = args != null ? args.y : null;
        });
        Test.GetSomethingElse.prototype.getQueryString = function () {
            var parameters = [];
            addParameter(parameters, 'id', this.id);
            addParameter(parameters, 'y', this.y);
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
            return '';
        };
        Test.GetSomethingElse.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/GetSomethingElse" + this.getQueryString();
        };
        Test.GetEnumerableString = (function (args) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
        Test.GetEnumerableString.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/GetEnumerableString";
        };
        Test.GetIHttpActionResult = (function (args) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
        Test.GetIHttpActionResult.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/GetIHttpActionResult";
        };
        Test.GetVoidTask = (function (args) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
        Test.GetVoidTask.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/GetVoidTask";
        };
        Test.GetStringTask = (function (args) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
        Test.GetStringTask.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/GetStringTask";
        };
        Test.GetEnumerableStringTask = (function (args) {
            this._verb = 'GET';
            this.hole = args != null ? args.hole : null;
        });
        Test.GetEnumerableStringTask.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/GetEnumerableStringTask";
        };
        Test.Post = (function (args) {
            this._verb = 'POST';
            this.hole = args != null ? args.hole : null;
        });
        Test.Post.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions";
        };
        Test.Post1 = (function (args) {
            this._verb = 'POST';
            this.hole = args != null ? args.hole : null;
        });
        Test.Post1.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/derived";
        };
        Test.Post2 = (function (args) {
            this._verb = 'POST';
            this.hole = args != null ? args.hole : null;
        });
        Test.Post2.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/derivedAgain";
        };
        Test.Put = (function (args) {
            this._verb = 'PUT';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
        });
        Test.Put.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions/" + this.id;
        };
        Test.Delete = (function (args) {
            this._verb = 'DELETE';
            this.id = args != null ? args.id : null;
            this.hole = args != null ? args.hole : null;
        });
        Test.Delete.prototype.getQueryString = function () {
            var parameters = [];
            addParameter(parameters, 'id', this.id);
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
            return '';
        };
        Test.Delete.prototype.toString = function () {
            return "/api/Test/" + this.hole + "/actions" + this.getQueryString();
        };
    })(Test = Endpoints.Test || (Endpoints.Test = {}));
    var Thingy;
    (function (Thingy) {
        Thingy.GetAll = (function (args) {
            this._verb = 'GET';
        });
        Thingy.GetAll.prototype.toString = function () {
            return "/api/thingy";
        };
        Thingy.Get = (function (args) {
            this._verb = 'GET';
            this.id = args != null ? args.id : null;
            this.x = args != null ? args.x : null;
            this.c = args != null ? args.c : null;
        });
        Thingy.Get.prototype.getQueryString = function () {
            var parameters = [];
            addParameter(parameters, 'x', this.x);
            addParameter(parameters, 'c', this.c);
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
            return '';
        };
        Thingy.Get.prototype.toString = function () {
            return "/api/thingy/" + this.id + this.getQueryString();
        };
        Thingy.Getty = (function (args) {
            this._verb = 'GET';
            this.y = args != null ? args.y : null;
            this.x = args != null ? args.x : null;
        });
        Thingy.Getty.prototype.getQueryString = function () {
            var parameters = [];
            addParameter(parameters, 'x', this.x);
            addParameter(parameters, 'y', this.y);
            if (parameters.length > 0) {
                return '?' + parameters.join('&');
            }
            return '';
        };
        Thingy.Getty.prototype.toString = function () {
            return "/api/thingy" + this.getQueryString();
        };
        Thingy.Post = (function (args) {
            this._verb = 'POST';
        });
        Thingy.Post.prototype.toString = function () {
            return "/api/thingy";
        };
    })(Thingy = Endpoints.Thingy || (Endpoints.Thingy = {}));
})(Endpoints || (Endpoints = {}));
//# sourceMappingURL=Endpoints.js.map