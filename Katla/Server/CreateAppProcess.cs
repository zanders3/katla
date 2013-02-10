using System;
using System.IO;
using System.Threading;

namespace zanders3.Katla.Server
{
    public static class CreateAppProcess
    {
        public static void CreateApp(string appName, Action<string> logMessage)
        {
            // Create the app container
            logMessage("----> Creating container");
            ProcessHelper.Run(logMessage, "lxc-create", "-t", "ubuntu", "-n", appName);

            // Complete!
            logMessage("----> Creation Completed");
        }
    }
}

