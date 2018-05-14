using System.Collections.Generic;

namespace WebApiToTypeScript.Config
{
    public class Config
    {
        // Single module to scan
        public string WebApiModuleFileName { get; set; }

        // List of modules to scan
        public string[] WebApiModuleFileNames { get; set; }

        // If true, will generate endpoints
        public bool GenerateEndpoints { get; set; }
            = false;

        // If true, will write return types for endpoints
        public bool GenerateEndpointsReturnTypes { get; set; }
            = false;

        // Directory name for endpoints
        public string EndpointsOutputDirectory { get; set; }
            = "Generated";

        // If true, will generate cached endpoints as well
        public bool EndpointsSupportCaching { get; set; }
            = false;

        // File name for endpoints
        public string EndpointsFileName { get; set; }
            = "Endpoints.ts";

        // Namespace for endpoints
        public string EndpointsNamespace { get; set; }
            = "Endpoints";

        // If true, will generate service
        public bool GenerateService { get; set; }
            = false;

        // Specifies the type of service to be generated
        //   Supported values: AngularJS, Angular, Axios
        public string ServiceType { get; set; }
            = "AngularJS";

        // Directory name for service
        public string ServiceOutputDirectory { get; set; }
            = "Generated";

        // File name for service
        public string ServiceFileName { get; set; }
            = "Service.ts";

        // Namespace for endpoints service
        public string ServiceNamespace { get; set; }
            = "Endpoints";

        // Name for service
        public string ServiceName { get; set; }
            = "AngularEndpointsService";

        // If true, will generate enums
        public bool GenerateEnums { get; set; }
            = true;

        // If true, will generate enum descriptions using the [Description] attribute
        public bool GenerateEnumDescriptions { get; set; }
            = false;

        // Directory name for enums
        public string EnumsOutputDirectory { get; set; }
            = "Generated";

        // File name to output enums
        public string EnumsFileName { get; set; }
            = "Enums.ts";

        // Namespace for enums
        public string EnumsNamespace { get; set; }
            = "Enums";

        // Any matching enums, even if not used in types used in controller actions
        //   will be generated
        public List<MatchConfigWithBaseType> EnumMatches { get; set; }
            = new List<MatchConfigWithBaseType>();

        // If true, will generate interfaces
        public bool GenerateInterfaces { get; set; }
            = true;

        // If turue, will generate interface classes
        public bool GenerateInterfaceClasses { get; set; }
            = true;

        // Directory name to output interfaces
        public string InterfacesOutputDirectory { get; set; }
            = "Generated";

        // File name to output interfaces
        public string InterfacesFileName { get; set; }
            = "Interfaces.ts";

        // Namespace for interfaces
        public string InterfacesNamespace { get; set; }
            = "Interfaces";

        // If true, all interface members will be generated in camelCase
        public bool InterfaceMembersInCamelCase { get; set; }
            = true;

        // Any property that has this custom attribute will be written in camelCase
        public string InterfaceCamelCaseCustomAttribute { get; set; }
            = null;

        // Any interface in the entire type pool that matches these configs will be created
        //   As in, even if they are not used by controllers
        //   Unless they are excluded by config InterfaceExcludeMatches
        public List<MatchConfigWithBaseType> InterfaceMatches { get; set; }
            = new List<MatchConfigWithBaseType>();

        // Any interface that matches any of these will not be created
        //   Including those used in controllers, and those matched by config InterfaceMatch
        public List<MatchConfigWithBaseType> InterfaceExcludeMatches { get; set; }
            = new List<MatchConfigWithBaseType>();

        // If true, will generate views
        public bool GenerateViews { get; set; }
            = false;

        // List of view configs
        public List<ViewConfig> ViewConfigs { get; set; }
            = new List<ViewConfig>();

        // Pattern for locating view files
        public string ViewsPattern { get; set; }
            = ".view.";

        // List of extensions to be ignored
        public string[] ViewsIgnoredExtensions { get; set; }
            = { };

        // Directory name for views
        public string ViewsOutputDirectory { get; set; }
            = "Generated";

        // If true, will group views under "Views"
        public bool UseViewsGroupingNamespace { get; set; }
            = true;

        // If true, will generate resources
        public bool GenerateResources { get; set; }
            = false;

        // List of resource configs
        public List<ResourceConfig> ResourceConfigs { get; set; }
            = new List<ResourceConfig>();

        // Namespace for resources
        public string ResourcesNamespace { get; set; }
            = "Resources";

        // Directory name for resources
        public string ResourcesOutputDirectory { get; set; }
            = "Generated";

        // If true, will scan other modules
        public bool ScanOtherModules { get; set; }
            = true;

        // If true, will write module instead of namespaces
        public bool WriteNamespaceAsModule { get; set; }
            = false;

        // When set, it will ignore all namespace names and generate imports instead as needed
        public bool NoNamespacesOrModules { get; set; }
            = false;

        public string NamespaceOrModuleName
            => WriteNamespaceAsModule ? "module" : "namespace";

        // List of type mappings to override C# types
        public List<TypeMapping> TypeMappings { get; set; }
            = new List<TypeMapping>();

        public string RoutePrefix { get; set; }
            = "api/";
    }
}