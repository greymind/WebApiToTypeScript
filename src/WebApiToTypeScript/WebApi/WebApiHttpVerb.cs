namespace WebApiToTypeScript.WebApi
{
    public class WebApiHttpVerb
    {
        public static WebApiHttpVerb Get 
            = new WebApiHttpVerb("Get");

        public static WebApiHttpVerb Post
            = new WebApiHttpVerb("Post");

        public static WebApiHttpVerb Put
            = new WebApiHttpVerb("Put");

        public static WebApiHttpVerb Delete
            = new WebApiHttpVerb("Delete");

        public static WebApiHttpVerb[] Verbs = new[]
        {
            Get,
            Post,
            Put,
            Delete
        };

        public string Verb { get; }
        public string VerbAttribute { get; }
        public string VerbMethod { get; }

        public WebApiHttpVerb(string verb)
        {
            Verb = verb;
            VerbMethod = verb.ToUpper();
            VerbAttribute = $"Http{Verb}Attribute";
        }
    }
}