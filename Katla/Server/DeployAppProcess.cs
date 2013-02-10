using System;
using System.Linq;
using System.IO;
using ServiceStack.Common.Web;
using System.Collections.Generic;

namespace zanders3.Katla.Server
{
    public class DeployAppProcess
    {
        public static void Start(object state)
        {
            DeployAppStatusResponse request = (DeployAppStatusResponse)state;
            try
            {
                Action<string> logMessage = line =>
                {
                    lock (request)
                    {
                        request.Builder.AppendLine(line);
                    }
                };

                // Create deployment directory
                logMessage("----> Create deployment directory");
                string targetDir = "/var/lib/lxc/" + request.AppName + "/rootfs/home/ubuntu/";
                if (!Directory.Exists(targetDir))
                    throw HttpError.NotFound("App not found: " + request.AppName);
                
                string deployDir = targetDir + "web";
                Console.WriteLine("Deploy to: " + deployDir);
                if (Directory.Exists(deployDir))
                    Directory.Delete(deployDir, true);
                Directory.CreateDirectory(deployDir);

                // Extract files to directory
                logMessage("----> Extract files (" + ((float)request.Contents.Length / (1024.0f * 1024.0f)) + " KB)");
                CompressionHelper.ExtractFolderFromStream(logMessage, request.Contents, deployDir);

                // Detect and install dependencies
                logMessage("----> Install app dependencies");
                DependencyChecker.CheckAndInstall(logMessage, deployDir, "/var/lib/lxc/" + request.AppName + "/rootfs/");

                // Create web startup job
                logMessage("----> Placing startup job");
                if (!File.Exists(deployDir + "/Procfile"))
                    throw HttpError.NotFound("Missing Procfile (" + deployDir + "/Procfile)");

                string startupJob = "/var/lib/lxc/" + request.AppName + "/rootfs/etc/init/web.conf";
                logMessage(startupJob);

                string[] startupJobContent = new string[]
                {
                    "start on net-device-up IFACE=eth0",
                    "respawn",
                    "script",
                    "cd /home/ubuntu/web",
                    "chmod +x Procfile",
                    "./Procfile",
                    "end script",
                    "console output"
                };
                File.WriteAllText(startupJob, string.Join("\n", startupJobContent));

                //Start the container
                logMessage("-----> Starting container");
                ProcessHelper.Run(logMessage, "lxc-stop", "-n", request.AppName);
                ProcessHelper.Run(logMessage, "lxc-start", "-d", "-n", request.AppName);

                //Generate the nginx configuration file
                logMessage("------> Updating and restarting nginx");
                Nginx.Configure(logMessage);

                //TODO: make an API call to the hosting system to add the hostname as a CNAME to point to katla.3zanders.co.uk

                logMessage("-----> App deployed");
                lock (request)
                {
                    request.Completed = true;
                }
            } 
            catch (Exception e)
            {
                lock (request)
                {
                    request.Exception = e;
                }
            }
        }
    }
}

