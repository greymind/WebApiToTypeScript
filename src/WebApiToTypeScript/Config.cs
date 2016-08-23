using System.Collections.Generic;

namespace WebApiToTypeScript
{
    public class Config
    {
        public string WebApiApplicationAssembly { get; set; }

        public string OutputDirectory { get; set; }

        public string EndpointFileName { get; set; }
            = "Endpoints.ts";

        public string Namespace { get; set; }
            = "Endpoints";

        public bool WriteAsModule { get; set; }
            = false;

        public bool GenerateInterfaces { get; set; }
            = false;

        public List<TypeMapping> TypeMappings { get; set; }
            = new List<TypeMapping>();
    }
}