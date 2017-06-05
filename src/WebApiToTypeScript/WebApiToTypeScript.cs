using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.Endpoints;
using WebApiToTypeScript.Enums;
using WebApiToTypeScript.Interfaces;
using WebApiToTypeScript.Resources;
using WebApiToTypeScript.Types;
using WebApiToTypeScript.Views;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript
{
    public class WebApiToTypeScript : AppDomainIsolatedTask
    {
        private Stopwatch stopwatch;
                
        public const string IEndpoint = nameof(IEndpoint);

        public static Config.Config Config { get; private set; }
        public static EnumsService EnumsService { get; private set; }
        public static TypeService TypeService { get; private set; }
        public static InterfaceService InterfaceService { get; private set; }

        public static EndpointsService EndpointsService { get; private set; }
        public static AngularEndpointsService AngularEndpointsService { get; private set; }

        public static ViewsService ViewsService { get; private set; }
        public static ResourceService ResourceService { get; private set; }

        public static List<string> LogMessages { get; }
            = new List<string>();

        [Required]
        public string ConfigFilePath { get; set; }

        public override bool Execute()
        {
            InitializeServices();

            var apiControllers = TypeService.GetControllers(Config.WebApiModuleFileName);

            if (Config.GenerateViews)
            {
                StartAnalysis("views");

                foreach (var viewsBlock in ViewsService.GetBlocksForViews())
                    CreateFileForBlock(viewsBlock.TypeScriptBlock, Config.ViewsOutputDirectory, viewsBlock.Filename);

                StopAnalysis();
            }

            if (Config.GenerateResources)
            {
                StartAnalysis("resources");

                foreach (var resourceBlock in ResourceService.GetBlocksForResources())
                    CreateFileForBlock(resourceBlock.TypeScriptBlock, Config.ResourcesOutputDirectory, resourceBlock.Filename);

                StopAnalysis();
            }

            if (Config.GenerateEndpoints || Config.GenerateService)
            {
                StartAnalysis("controllers and actions");

                var endpointBlock = EndpointsService.CreateEndpointBlock();
                var serviceBlock = AngularEndpointsService.CreateServiceBlock();

                foreach (var apiController in apiControllers)
                {
                    var webApiController = new WebApiController(apiController);

                    if (Config.GenerateEndpoints || Config.GenerateService)
                        EndpointsService.WriteEndpointClassToBlock(endpointBlock, webApiController);

                    if (Config.GenerateService)
                    {
                        var classBlock = serviceBlock.Children
                            .OfType<TypeScriptBlock>()
                            .First();

                        AngularEndpointsService.WriteServiceObjectToBlock(classBlock, webApiController);
                    }
                }

                if (Config.GenerateEndpoints || Config.GenerateService)
                {
                    CreateFileForBlock(endpointBlock, Config.EndpointsOutputDirectory, Config.EndpointsFileName);
                }

                if (Config.GenerateService)
                {
                    CreateFileForBlock(serviceBlock, Config.ServiceOutputDirectory, Config.ServiceFileName);
                }

                StopAnalysis();
            }

            if (Config.GenerateInterfaces)
            {
                StartAnalysis("interfaces");

                var interfacesBlock = InterfaceService.CreateInterfacesBlock();
                InterfaceService.AddMatchingInterfaces();
                InterfaceService.WriteInterfacesToBlock(interfacesBlock);

                StopAnalysis();

                CreateFileForBlock(interfacesBlock, Config.InterfacesOutputDirectory, Config.InterfacesFileName);
            }

            if (Config.GenerateEnums)
            {
                StartAnalysis("enumerations");

                var enumsBlock = EnumsService.CreateEnumsBlock();
                EnumsService.AddMatchingEnums();
                EnumsService.WriteEnumsToBlock(enumsBlock);

                StopAnalysis();

                CreateFileForBlock(enumsBlock, Config.EnumsOutputDirectory, Config.EnumsFileName);
            }

            WriteServiceLogMessages();

            return true;
        }

        private void WriteServiceLogMessages()
        {
            LogMessage("");

            foreach (var message in LogMessages)
                LogMessage(message);
        }

        private void InitializeServices()
        {
            Config = GetConfig(ConfigFilePath);

            TypeService = new TypeService();
            TypeService.LoadAllTypes(Config.WebApiModuleFileName);

            EnumsService = new EnumsService();
            InterfaceService = new InterfaceService();

            EndpointsService = new EndpointsService();
            AngularEndpointsService = new AngularEndpointsService();

            ViewsService = new ViewsService();
            ResourceService = new ResourceService();
        }

        private static string ToAbsolutePath(string baseDir, string directory)
        {
            return Path.GetFullPath(Path.Combine(baseDir, directory));
        }

        private Config.Config GetConfig(string configFilePath)
        {
            var configFileContent = File.ReadAllText(configFilePath);

            var config = JsonConvert.DeserializeObject<Config.Config>(configFileContent);

            var baseDir = Path.GetFullPath(Path.GetDirectoryName(configFilePath) ?? "");

            config.WebApiModuleFileName = ToAbsolutePath(baseDir, config.WebApiModuleFileName);
            config.EndpointsOutputDirectory = ToAbsolutePath(baseDir, config.EndpointsOutputDirectory);
            config.ServiceOutputDirectory = ToAbsolutePath(baseDir, config.ServiceOutputDirectory);
            config.EnumsOutputDirectory = ToAbsolutePath(baseDir, config.EnumsOutputDirectory);
            config.InterfacesOutputDirectory = ToAbsolutePath(baseDir, config.InterfacesOutputDirectory);
            config.ViewsOutputDirectory = ToAbsolutePath(baseDir, config.ViewsOutputDirectory);
            config.ResourcesOutputDirectory = ToAbsolutePath(baseDir, config.ResourcesOutputDirectory);

            foreach (var viewConfig in config.ViewConfigs)
                viewConfig.SourceDirectory = ToAbsolutePath(baseDir, viewConfig.SourceDirectory);

            foreach (var resourceConfig in config.ResourceConfigs)
                resourceConfig.SourcePath = ToAbsolutePath(baseDir, resourceConfig.SourcePath);

            return config;
        }

        private void CreateFileForBlock(TypeScriptBlock typeScriptBlock, string outputDirectory, string fileName)
        {
            LogMessage($"Writing {fileName}...");

            CreateOuputDirectory(outputDirectory);

            var filePath = Path.Combine(outputDirectory, fileName);

            using (var endpointFileWriter = new StreamWriter(filePath, false))
            {
                endpointFileWriter.Write(typeScriptBlock.ToString());
            }
        }

        private void CreateOuputDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void StartAnalysis(string ofWhat)
        {
            LogMessage("");
            LogMessage($"Analyzing {ofWhat}...");

            stopwatch = Stopwatch.StartNew();
        }

        private void StopAnalysis()
        {
            LogMessage($"Analysis complete. Took {stopwatch.ElapsedMilliseconds}ms.");

            stopwatch = null;
        }

        private void LogMessage(string message)
        {
            try
            {
                Log.LogMessage(message);
            }
            catch (Exception)
            {
                Console.WriteLine(message);
            }
        }
    }
}