using System;
using System.IO;
using ServiceStack.Common.Web;

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
                logMessage("----> Extract files");
                CompressionHelper.ExtractFolderFromString(logMessage, request.Contents, deployDir);

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


                // Start a container
                logMessage("-----> Starting a container");
                ProcessHelper.Run(logMessage, "lxc-start-ephemeral", "-d", "-o", request.AppName);

                //TODO: system to spin up, scale and track instances.
                //TODO: copy config file and append lxc.network.ipv4 = 10.0.3.X
                //TODO: update nginx config file and restart nginx.
                //TODO: add routing system via route 53 (which then load balances to multiple boxes. WHOA.)

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

