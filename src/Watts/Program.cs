using System;

namespace Watts
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: Watts.exe <\"Path/To/Config.json\">");
                return 1;
            }

            var watts = new WebApiToTypeScript.WebApiToTypeScript
            {
                ConfigFilePath = args[0]
            };

            watts.Execute();

#if DEBUG
            Console.ReadLine();
#endif

            return 0;
        }
    }
}