using System.Collections.Generic;

namespace WebApiToTypeScript.Config
{
    public class Config
    {
        public string WebApiModuleFileName { get; set; }

        public string EndpointsOutputDirectory { get; set; }
            = "Endpoints";

        public string EndpointsFileName { get; set; }
            = "Endpoints.ts";

        public string EndpointsNamespace { get; set; }
            = "Endpoints";

        public bool GenerateService { get; set; }
            = true;

        public string ServiceOutputDirectory { get; set; }
            = "Endpoints";

        public string ServiceFileName { get; set; }
            = "Service.ts";

        public string ServiceNamespace { get; set; }
            = "Endpoints";

        public string ServiceName { get; set; }
            = "AngularEndpointsService";

        public bool GenerateEnums { get; set; }
            = true;

        public string EnumsOutputDirectory { get; set; }
            = "Enums";

        public string EnumsFileName { get; set; }
            = "Enums.ts";

        public string EnumsNamespace { get; set; }
            = "Enums";

        public bool GenerateInterfaces { get; set; }
            = true;

        public string InterfacesOutputDirectory { get; set; }
            = "Interfaces";

        public string InterfacesFileName { get; set; }
            = "Interfaces.ts";

        public string InterfacesNamespace { get; set; }
            = "Interfaces";

        public bool InterfaceMembersInCamelCase { get; set; }
            = true;

        public List<InterfaceMatch> InterfaceMatches { get; set; }
            = new List<InterfaceMatch>();

        public bool GenerateViews { get; set; }
            = true;

        public string ViewsSourceDirectory { get; set; }
            = "Views";

        public string ViewsOutputDirectory { get; set; }
            = "Views";

        public string ViewsFileName { get; set; }
            = "Views.ts";

        public string ViewsNamespace { get; set; }
            = "Views";

        public bool ScanOtherModules { get; set; }
            = true;

        public bool WriteNamespaceAsModule { get; set; }
            = false;

        public string NamespaceOrModuleName
            => WriteNamespaceAsModule ? "module" : "namespace";

        public List<TypeMapping> TypeMappings { get; set; }
            = new List<TypeMapping>();
    }
}