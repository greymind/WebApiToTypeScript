namespace WebApiToTypeScript.Config
{
    public class TypeMapping
    {
        public string WebApiTypeName { get; set; }
        public string TypeScriptTypeName { get; set; }

        public bool TreatAsAttribute { get; set; }
            = false;

        public bool TreatAsConstraint { get; set; }
            = false;

        public bool AutoInitialize { get; set; }
            = false;

        public string Match { get; set; }
    }
}