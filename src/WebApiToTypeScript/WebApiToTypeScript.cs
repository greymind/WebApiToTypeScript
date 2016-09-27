using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WebApiToTypeScript.Block;
using WebApiToTypeScript.Endpoints;
using WebApiToTypeScript.Enums;
using WebApiToTypeScript.Interfaces;
using WebApiToTypeScript.Types;
using WebApiToTypeScript.WebApi;

namespace WebApiToTypeScript
{
    public class WebApiToTypeScript : AppDomainIsolatedTask
    {
        private Stopwatch stopwatch;

        public const string IHaveQueryParams = nameof(IHaveQueryParams);
        public const string IEndpoint = nameof(IEndpoint);

        public static Config.Config Config { get; private set; }
        public static EnumsService EnumsService { get; private set; }
        public static TypeService TypeService { get; private set; }
        public static InterfaceService InterfaceService { get; private set; }

        public static EndpointsService EndpointsService { get; private set; }
        public static AngularEndpointsService AngularEndpointsService { get; private set; }

        [Required]
        public string ConfigFilePath { get; set; }

        public override bool Execute()
        {
            InitializeServices();

            var apiControllers = TypeService.GetControllers(Config.WebApiModuleFileName);

            var endpointBlock = EndpointsService.CreateEndpointBlock();
            var serviceBlock = AngularEndpointsService.CreateServiceBlock();

            StartAnalysis("controllers and actions");

            foreach (var apiController in apiControllers)
            {
                var webApiController = new WebApiController(apiController);

                EndpointsService.WriteEndpointClassToBlock(endpointBlock, webApiController);
                AngularEndpointsService.WriteServiceObjectToBlock(serviceBlock.Children.First() as TypeScriptBlock, webApiController);
            }

            StopAnalysis();

            CreateFileForBlock(endpointBlock, Config.EndpointsOutputDirectory, Config.EndpointsFileName);
            CreateFileForBlock(serviceBlock, Config.ServiceOutputDirectory, Config.ServiceFileName);

            var enumsBlock = EnumsService.CreateEnumsBlock();
            var interfacesBlock = InterfaceService.CreateInterfacesBlock();

            if (Config.GenerateInterfaces)
            {
                StartAnalysis("interfaces");

                InterfaceService.AddMatchingInterfaces();
                InterfaceService.WriteInterfacesToBlock(interfacesBlock);

                StopAnalysis();

                CreateFileForBlock(interfacesBlock, Config.InterfacesOutputDirectory, Config.InterfacesFileName);
            }

            if (Config.GenerateEnums)
            {
                StartAnalysis("enumerations");

                EnumsService.WriteEnumsToBlock(enumsBlock);

                StopAnalysis();

                CreateFileForBlock(enumsBlock, Config.EnumsOutputDirectory, Config.EnumsFileName);
            }

            return true;
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
        }

        private Config.Config GetConfig(string configFilePath)
        {
            var configFileContent = File.ReadAllText(configFilePath);

            return JsonConvert.DeserializeObject<Config.Config>(configFileContent);
        }

        private void CreateFileForBlock(TypeScriptBlock typeScriptBlock, string outputDirectory, string fileName)
        {
            CreateOuputDirectory(outputDirectory);

            var filePath = Path.Combine(outputDirectory, fileName);

            //LogMessage($"Writing file [{filePath}]...");

            //stopwatch = Stopwatch.StartNew();

            using (var endpointFileWriter = new StreamWriter(filePath, false))
            {
                endpointFileWriter.Write(typeScriptBlock.ToString());
            }

            //LogMessage($"File [{filePath}] saved! Took {stopwatch.ElapsedMilliseconds}ms.");
        }

        private void CreateOuputDirectory(string directory)
        {
            //LogMessage($"Creating directory [{directory}]...");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                //LogMessage($"Directory [{directory}] created!");
            }
            else
            {
                //LogMessage($"Directory [{directory}] already exists!");
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

        private void LogMessage(string log)
        {
            try
            {
                Log.LogMessage(log);
            }
            catch (Exception)
            {
                Console.WriteLine(log);
            }
        }
    }
}