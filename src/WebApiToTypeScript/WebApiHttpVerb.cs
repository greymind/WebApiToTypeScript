namespace WebApiToTypeScript
{
    public class WebApiHttpVerb
    {
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