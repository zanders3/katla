using System;
using System.IO;
using System.Threading;

namespace zanders3.Katla
{
    public static class CreateAppProcess
    {
        public static void CreateApp(string appName, string buildScript, Action<string> logMessage)
        {
            //1. Create the app container
            logMessage("----> Creating container");
            ProcessHelper.Run(logMessage, "lxc-create", "-t", "ubuntu", "-n", appName);

            //2. Create build script file
            logMessage("----> Preparing build script");
            string fileName = "/var/lib/lxc/"+ appName + "/rootfs/home/ubuntu/build.sh";
            File.WriteAllText(fileName, buildScript);
            if (!ProcessHelper.Run(logMessage, "chmod", "+x", fileName))
                return;

            //3. Run build script inside container
            logMessage("----> Starting " + appName + " container");
            if (!ProcessHelper.Run(logMessage, "lxc-start", "-n", appName, "-d"))
                return;

            logMessage("----> Running build script");
            ProcessHelper.Run(logMessage, "chroot", "/var/lib/lxc/" + appName + "/rootfs/", "/home/ubuntu/build.sh");

            //4. Shutdown container
            logMessage("----> Shutting down container");
            if (!ProcessHelper.Run(logMessage, "lxc-stop", "-n", appName))
                return;

            //5. Complete!
            logMessage("----> Built Sucessfully");
        }
    }
}

