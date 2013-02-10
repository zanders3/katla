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
            IEnumerable<string> appNames = Directory.GetDirectories("/var/lib/lxc/").Select(folder => Path.GetFileName(folder));

            List<string> fileLines = appNames.SelectMany(app =>
            {
                return new List<string>()
                {
                    "server {",
                    "\tlisten\t80;",
                    "\tserver_name " + app + ".katla.3zanders.co.uk;",
                    "\tlocation / {",
                    "\t\tproxy_pass http://" + app + ":8080;",
                    "\t}",
                    "}"
                }; 
            }).ToList();
            fileLines.AddRange(new List<string>()
            {
                "server {",
                "\tlisten\t80;",
                "\tsendfile\ton;",
                "\tserver_name katla.3zanders.co.uk katla.cloudapp.net;",
                "\tlocation / {",
                "\t\tproxy_pass http://127.0.0.1:8080;",
                "\t\tclient_max_body_size 200M;",
                "\t}",
                "}"
            });
            File.WriteAllLines("/etc/nginx/conf.d/config.conf", fileLines);
            ProcessHelper.Run(logMessage, "sudo", "rm", "/etc/nginx/sites-enabled/default");
            ProcessHelper.Run(logMessage, "sudo", "service", "nginx", "restart");
        }
    }
}

