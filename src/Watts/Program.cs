using System;
using System.Diagnostics;
using System.IO;

namespace Watts
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var watts = new WebApiToTypeScript.WebApiToTypeScript();
            if (args.Length == 0)
            {
                var path = Path.Combine(Environment.CurrentDirectory, "watts.config.json");
                if (File.Exists(path))
                    watts.ConfigFilePath = path;
            }
            else if (args.Length > 0 && File.Exists(args[0]))
            {
                watts.ConfigFilePath = args[0];
            }

            int status = 0;
            if (watts.ConfigFilePath == null)
            {
                Console.WriteLine("Usage: Watts.exe <\"Path/To/Config.json\">");
                status = -1;
            }
            else
            {
                status = watts.Execute() ? 0 : -1;
            }

            if (Debugger.IsAttached)
            {
                Console.Write("Press any key to continue . . . ");
                Console.ReadKey();
            }

            return 0;
        }
    }
}