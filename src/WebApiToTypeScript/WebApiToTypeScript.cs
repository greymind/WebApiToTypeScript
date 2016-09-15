using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
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

            foreach (var apiController in apiControllers)
            {
                var webApiController = new WebApiController(apiController);

                EndpointsService.WriteEndpointClassToBlock(endpointBlock, webApiController);
                AngularEndpointsService.WriteServiceObjectToBlock(serviceBlock.Children.First() as TypeScriptBlock, webApiController);
            }

            CreateFileForBlock(endpointBlock, Config.EndpointsOutputDirectory, Config.EndpointsFileName);
            CreateFileForBlock(serviceBlock, Config.ServiceOutputDirectory, Config.ServiceFileName);

            var enumsBlock = EnumsService.CreateEnumsBlock();
            var interfacesBlock = InterfaceService.CreateInterfacesBlock();

            if (Config.GenerateInterfaces)
            {
                InterfaceService.AddMatchingInterfaces();
                InterfaceService.WriteInterfacesToBlock(interfacesBlock);

                CreateFileForBlock(interfacesBlock, Config.InterfacesOutputDirectory, Config.InterfacesFileName);
            }

            if (Config.GenerateEnums)
            {
                EnumsService.WriteEnumsToBlock(enumsBlock);

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

            using (var endpointFileWriter = new StreamWriter(filePath, false))
            {
                endpointFileWriter.Write(typeScriptBlock.ToString());
            }

            LogMessage($"{filePath} created!");
        }

        private void CreateOuputDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                LogMessage($"{directory} created!");
            }
            else
            {
                LogMessage($"{directory} already exists!");
            }
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