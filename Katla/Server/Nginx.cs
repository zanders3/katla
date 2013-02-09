using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace zanders3.Katla.Server
{
    public static class Nginx
    {
        public static void Configure(Action<string> logMessage)
        {
            List<string> fileLines = AppStatusModel.Get().Where(app => app.Running).SelectMany(app =>
            {
                return new List<string>()
                {
                    "server {",
                    "\tlisten\t80;",
                    "\tserver_name " + app.HostName + ";",
                    "\tlocation / {",
                    "\t\tproxy_pass http://" + app.InternalIP + ":8080;",
                    "\t}",
                    "}"
                }; 
            }).ToList();
            fileLines.AddRange(new List<string>()
            {
                "server {",
                "\tlisten\t80;",
                "\tserver_name katla.3zanders.co.uk katla.cloudapp.net;",
                "\tlocation / {",
                "\t\tproxy_pass http://127.0.0.1:8080;",
                "\t}",
                "}"
            });
            File.WriteAllLines("/etc/nginx/conf.d/config.conf", fileLines);

            ProcessHelper.Run(logMessage, "sudo", "rm", "/etc/nginx/sites-enabled/default");
            ProcessHelper.Run(logMessage, "sudo", "service", "nginx", "restart");
        }
    }
}

