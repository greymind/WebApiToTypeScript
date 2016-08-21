declare var $: JQueryStatic;
import TestEndpoint = Endpoints.TestEndpoint;

var endpoints: any[] = [
    new TestEndpoint.Get("cap"),
    new TestEndpoint.Get1(7, "cap"),
    new TestEndpoint.GetSomething("cap", 1, 2),
    new TestEndpoint.GetSomethingElse(3, "clap", "cap"),
    new TestEndpoint.Post("cap", "balki"),
    new TestEndpoint.Put(5, "boo", "cap"),
    new TestEndpoint.Delete(2, "cap")
];

endpoints.forEach(e => {
    let endpoint = e.toString();
    console.log(endpoint);

    $.ajax({
        method: e.verb,
        url: endpoint
    }).then((data, status) => {
        console.log(e.verb, endpoint, status, data);
    });
});