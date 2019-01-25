using System.Collections.Generic;

namespace WebApiToTypeScript.Config
{
    public class Config
    {
        public string WebApiModuleFileName { get; set; }

        public string[] WebApiModuleFileNames { get; set; }

        public bool GenerateEndpointsReturnTypes { get; set; }
            = false;

        public bool GenerateEndpoints { get; set; }
            = false;

        public bool GenerateMobileEndpoints { get; set; }
            = false;

        public string EndpointsOutputDirectory { get; set; }
            = "Generated";

        public string MobileEndpointsOutputDirectory { get; set; }
            = "GeneratedMobile";

        public bool EndpointsSupportCaching { get; set; }
            = false;

        public string EndpointsFileName { get; set; }
            = "Endpoints.ts";

        public string MobileEndpointsFileName { get; set; }
            = "MobileEndpoints.ts";

        public string EndpointsNamespace { get; set; }
            = "Endpoints";

        public string MobileEndpointsNamespace { get; set; }
            = "MobileEndpoints";

        public bool GenerateService { get; set; }
            = false;

        public string ServiceOutputDirectory { get; set; }
            = "Generated";

        public string ServiceFileName { get; set; }
            = "Service.ts";

        public string ServiceNamespace { get; set; }
            = "Endpoints";

        public string ServiceName { get; set; }
            = "AngularEndpointsService";

        public bool GenerateEnums { get; set; }
            = true;

        public bool GenerateEnumDescriptions { get; set; }
            = false;

        public string EnumsOutputDirectory { get; set; }
            = "Generated";

        public string EnumsFileName { get; set; }
            = "Enums.ts";

        public string EnumsNamespace { get; set; }
            = "Enums";

        public List<MatchConfig> EnumMatches { get; set; }
            = new List<MatchConfig>();

        public bool GenerateInterfaces { get; set; }
            = true;

        public bool GenerateInterfaceClasses { get; set; }
            = true;

        public string InterfacesOutputDirectory { get; set; }
            = "Generated";

        public string InterfacesFileName { get; set; }
            = "Interfaces.ts";

        public string InterfacesNamespace { get; set; }
            = "Interfaces";

        public bool InterfaceMembersInCamelCase { get; set; }
            = true;

        public string InterfaceCamelCaseCustomAttribute { get; set; }
            = null;

        public List<MatchConfig> InterfaceMatches { get; set; }
            = new List<MatchConfig>();

        public bool GenerateViews { get; set; }
            = false;

        public List<ViewConfig> ViewConfigs { get; set; }
            = new List<ViewConfig>();

        public string ViewsPattern { get; set; }
            = ".view.";

        public string[] ViewsIgnoredExtensions { get; set; }
            = { };

        public string ViewsOutputDirectory { get; set; }
            = "Generated";

        public bool UseViewsGroupingNamespace { get; set; }
            = true;

        public bool GenerateResources { get; set; }
            = false;

        public List<ResourceConfig> ResourceConfigs { get; set; }
            = new List<ResourceConfig>();

        public string ResourcesNamespace { get; set; }
            = "Resources";

        public string ResourcesOutputDirectory { get; set; }
            = "Generated";

        public bool ScanOtherModules { get; set; }
            = true;

        public bool WriteNamespaceAsModule { get; set; }
            = false;

        public string NamespaceOrModuleName
            => WriteNamespaceAsModule ? "module" : "namespace";

        public List<TypeMapping> TypeMappings { get; set; }
            = new List<TypeMapping>();

        public string MobileEndpointAttributeName { get; set; } 
            = "MobileEndpointAttribute";

        public Config ApplyCommandLineConfiguration(CommandLineConfig commandLineConfig)
        {
            if (commandLineConfig == null)
                return this;

            if (!string.IsNullOrEmpty(commandLineConfig.WebApiModuleFileNames))
                this.WebApiModuleFileNames = commandLineConfig.WebApiModuleFileNames.Split(',');

            return this;
        }
    }
}
