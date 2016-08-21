using System;

namespace Watts
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Watts.exe \"Path/To/WebApplication.dll\" \"Path/To/OutputFolder\"");
                return 1;
            }

            var watts = new WebApiToTypeScript.WebApiToTypeScript()
            {
                WebApiApplicationAssembly = args[0],
                OutputDirectory = args[1]
            };

            watts.Execute();

            return 0;
        }
    }
}