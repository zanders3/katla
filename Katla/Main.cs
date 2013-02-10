using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace zanders3.Katla
{
	class MainClass
	{
		static void PrintHelp()
		{
			Console.WriteLine ("Usage:\n\tkatla create <appname> <buildscript>\n\tCreates a katla application on the server.");
			Console.WriteLine("\n\tkatla deploy <appname>\n\tDeploys the current folder into the application on the server.");
			Console.WriteLine("\n\tkatla server <endpoint> <port>\n\tRuns the katla server");
		}

        const string Endpoint = /*"http://localhost:8080/";*/"http://katla.3zanders.co.uk/";

        private static IEnumerable<Assembly> GetDependentAssemblies(Assembly analyzedAssembly)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => GetNamesOfAssembliesReferencedBy(a)
                       .Contains(analyzedAssembly.FullName));
        }
        
        public static IEnumerable<string> GetNamesOfAssembliesReferencedBy(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.FullName);
        }

		public static void Main(string[] args)
		{
            foreach (string assembly in GetNamesOfAssembliesReferencedBy(Assembly.GetExecutingAssembly()))
                Console.WriteLine(assembly);

            if (args.Length == 0)
			{
				PrintHelp();
				return;
			}
			
			switch (args[0]) 
			{
			case "create":
				if (args.Length != 3 || !File.Exists(args[2]))
				{
					PrintHelp();
					return;
				}
				
				CreateAppClient.CreateApp(Endpoint, args[1], args[2]);
				break;
			case "deploy":
                if (args.Length != 2)
                {
                    PrintHelp();
                    return;
                }
                
                DeployAppClient.DeployApp(Endpoint, args[1]);
				break;
			case "server":
                if (args.Length != 3)
                {
                    PrintHelp();
                    return;
                }
                
				Server.KatlaServer.Start("http://" + args[1] + ":" + args[2] + "/");
				break;
            default:
                Console.WriteLine("Unknown command: " + args[0]);
                break;
			}
		}
	}
}
