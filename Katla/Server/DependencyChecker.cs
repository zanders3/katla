using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace zanders3.Katla.Server
{
    public static class DependencyChecker
    {
        public static void CheckAndInstall(Action<string> logMessage, string deployDir, string rootFolder)
        {
            logMessage("Reading libraries...");
            Dictionary<string, List<string>> dllToPackage = new Dictionary<string, List<string>>();
            foreach (string packageList in Directory.GetFiles("/var/lib/dpkg/info/", "libmono-*.list"))
            {
                string packageName = Path.GetFileNameWithoutExtension(packageList);
                foreach (string file in File.ReadAllLines(packageList).Where(file => file.EndsWith(".dll")))
                {
                    string dllName = Path.GetFileNameWithoutExtension(file);
                    logMessage(dllName);
                    if (!dllToPackage.ContainsKey(dllName))
                        dllToPackage.Add(dllName, new List<string>());
                    
                    if (!dllToPackage[dllName].Contains(packageName))
                        dllToPackage[dllName].Add(packageName);
                }
            }

            List<string> softwareToInstall = new List<string>();
            foreach (string file in Directory.GetFiles(deployDir, "*.exe"))
            {
                logMessage("");
                logMessage("Gathering dependencies for " + file + "...");
                string version = string.Empty;
                List<string> dependencies = new List<string>();
                Action<string> parseDependencies = line =>
                {
                    if (line.Contains("Version="))
                    {
                        version = line.Substring(line.IndexOf("Version=") + "Version=".Length);
                    }
                    else if (line.Contains("Name="))
                    {
                        string name = line.Substring(line.IndexOf("Name=") + "Name=".Length);
                        logMessage(name + " v " + version);
                        if (dllToPackage.ContainsKey(name))
                        {
                            logMessage("\t" + string.Join("\n\t", dllToPackage[name]));
                            dependencies.AddRange(dllToPackage[name]);
                        }
                    }
                };
                ProcessHelper.Run(parseDependencies, "monodis", "--assemblyref", file);

                logMessage("");
                logMessage(file + " depends on:");
                logMessage("----> " + string.Join(" ", dependencies));

                foreach (string dependency in dependencies)
                    if (!softwareToInstall.Contains(dependency))
                        softwareToInstall.Add(dependency);
            }

            logMessage("");
            logMessage("Installing dependencies....");

            string[] dependencyInstallerScript = new string[]
            {
                "#!/bin/sh",
                "export DEBIAN_FRONTEND=noninteractive",
                "apt-get -q -y install " + string.Join(" ", softwareToInstall),
                ""
            };
            string installerScript = Path.Combine(rootFolder, "home/ubuntu/install.sh");
            File.WriteAllText(installerScript, string.Join("\n", dependencyInstallerScript));
            ProcessHelper.Run(logMessage, "chmod", "+x", installerScript);
            ProcessHelper.Run(logMessage, "chroot", rootFolder, "/home/ubuntu/install.sh");
        }
    }
}

