using CommandLine;

namespace WebApiToTypeScript.Config
{
    public class CommandLineConfig
    {
        [Option("webapimodulefilenames", Required = false, HelpText = "Comma separated web api module file names.")]
        public string WebApiModuleFileNames { get; set; }
    }
}
