using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using System.IO;

namespace zanders3.Katla.Server
{
    [Route("/API/Deploy")]
    public class DeployAppRequest : IReturnVoid
    {
        public string AppName { get; set; }
        public string Contents { get; set; }
    }

    public class DeployAppService : Service
    {
        public void Post(DeployAppRequest request)
        {
            string targetDir = "/var/lib/lxc/" + request.AppName + "/rootfs/home/ubuntu/";
            if (!Directory.Exists(targetDir))
                throw HttpError.NotFound("App not found: " + request.AppName);

            string deployDir = targetDir + "deploy";
            Console.WriteLine("Deploying to: " + deployDir);
            if (Directory.Exists(deployDir))
                Directory.Delete(deployDir, true);
            Directory.CreateDirectory(deployDir);

            CompressionHelper.ExtractFolderFromString(request.Contents, deployDir);
        }
    }
}

