using System;
using System.IO;
using System.Threading;

namespace zanders3.Katla
{
    public static class CreateAppProcess
    {
        public static void CreateApp(string appName, string buildScript, Action<string> logMessage)
        {
            // Create the app container
            logMessage("----> Creating container");
            ProcessHelper.Run(logMessage, "lxc-create", "-t", "ubuntu", "-n", appName);

            // Create build script file
            logMessage("----> Preparing build script");
            string fileName = "/var/lib/lxc/"+ appName + "/rootfs/home/ubuntu/build.sh";
            File.WriteAllText(fileName, buildScript);
            if (!ProcessHelper.Run(logMessage, "chmod", "+x", fileName))
                return;
        
            // Run build script inside container
            logMessage("----> Running build script");
            ProcessHelper.Run(logMessage, "chroot", "/var/lib/lxc/" + appName + "/rootfs/", "/home/ubuntu/build.sh");

            // Complete!
            logMessage("----> Built Sucessfully");
        }
    }
}

