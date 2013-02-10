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

                // Create web startup job
                logMessage("----> Placing startup job");
                if (!File.Exists("/var/lib/lxc/" + request.AppName + "/rootfs/home/ubuntu/web/startup.sh"))
                    throw HttpError.Conflict("startup.sh script not found. I don't know how to start the service.");
                ProcessHelper.Run(logMessage, "chmod", "+x", "/var/lib/lxc/" + request.AppName + "/rootfs/home/ubuntu/web/startup.sh");

                logMessage("/var/lib/lxc/" + request.AppName + "/rootfs/etc/init/web.conf");
                File.WriteAllText(
                    "/var/lib/lxc/" + request.AppName + "/rootfs/etc/init/web.conf", 
                    "start on net-device-up IFACE=eth0\nrespawn\nexec /home/ubuntu/web/startup.sh\nconsole output"
                );

                // Assign container internal static ip address
                logMessage("----> Writing container configuration file");
                AppStatus status = AppStatusModel.Get(request.AppName);
                status.InternalIP = "10.0.3." + (status.ID + 1);
                status.HostName  = status.AppName + ".katla.3zanders.co.uk";

                List<string> configFile = 
                    File.ReadAllLines("/var/lib/lxc/" + request.AppName + "/config")
                    .Where(line => !line.Contains("lxc.network.ipv4"))
                    .ToList();
                configFile.Add("lxc.network.ipv4 = " + status.InternalIP);

                File.WriteAllLines("/var/lib/lxc/" + request.AppName + "/config", configFile);

                //Start the container
                logMessage("-----> Starting container");
                ProcessHelper.Run(logMessage, "lxc-stop", "-n", request.AppName);
                ProcessHelper.Run(logMessage, "lxc-start", "-d", "-n", request.AppName);

                //Generate the nginx configuration file
                logMessage("------> Updating and restarting nginx");
                status.Running = true;

                AppStatusModel.Save(status);
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

