namespace WebApiToTypeScript.Config
{
    public class TypeMapping
    {
        // Name of type in WebAPI
        public string WebApiTypeName { get; set; }

        // Name of type in TypeScript
        public string TypeScriptTypeName { get; set; }

        // If ture, treat as attribute
        public bool TreatAsAttribute { get; set; }
            = false;

        // If true, treats type as constraint
        public bool TreatAsConstraint { get; set; }
            = false;

        // If true, treats type as enum
        public bool TreatAsEnum { get; set; }
            = false;

        // If true, automatically initializes the type in TypeScript
        public bool AutoInitialize { get; set; }
            = false;

        // Match regex
        public string Match { get; set; }

        public override string ToString()
        {
            return $"{WebApiTypeName} -> {TypeScriptTypeName}";
        }
    }
}