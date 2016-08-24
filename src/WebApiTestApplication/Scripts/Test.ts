class TestObject implements Endpoints.IHaveQueryParams {
    Name = "something"
    Date = new Date().toJSON()

    getQueryParams() {
        return this;
    }
}

var testObject = new Interfaces.DummyClass();
var b = {
    getQueryParams() {
        return {
            Name: "asd",
            Date: new Date().toJSON()
        }
    }
}

var endpoints: any[] = [
    new Endpoints.Test.Get("cap"),
    new Endpoints.Test.Get1("7", "cap"),
    new Endpoints.Test.GetSomething(7, "cap", 2),
    new Endpoints.Test.GetSomethingElse(3, testObject, "cap"),
    //new Endpoints.Test.GetSomethingElse(3, "asd", "cap"),
    //new Endpoints.Test.GetSomethingElse(3, 0, "cap"),
    //new Endpoints.Test.GetSomethingElse(3, b, "cap"),
    new Endpoints.Test.Post("cap"),
    new Endpoints.Test.Put(5, "cap"),
    new Endpoints.Test.Delete(2, "cap")
];

endpoints.forEach(e => {
    let endpoint = e.toString();
    console.log(endpoint);

    let request: JQueryAjaxSettings = {
        method: e.verb,
        url: endpoint,
    }

    if (e.verb === "POST" || e.verb === "PUT") {
        request.data = {
            "": "balkithewise"
        };
    }

    $.ajax(request).then((data, status) => {
        console.log(e.verb, endpoint, status, data);
    });
});