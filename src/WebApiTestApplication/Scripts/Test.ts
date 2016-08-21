declare var $: JQueryStatic;
import TestEndpoint = Endpoints.TestEndpoint;

var endpoints: any[] = [
    new TestEndpoint.Get("cap"),
    new TestEndpoint.Get1(7, "cap"),
    new TestEndpoint.GetSomething("cap", 1, 2),
    new TestEndpoint.GetSomethingElse(3, "clap", "cap"),
    new TestEndpoint.Post("cap"),
    new TestEndpoint.Put(5, "cap"),
    new TestEndpoint.Delete(2, "cap")
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