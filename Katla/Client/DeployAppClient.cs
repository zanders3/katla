using System;
using System.IO;
using ServiceStack.ServiceClient.Web;
using System.IO.Compression;
using zanders3.Katla.Server;

namespace zanders3.Katla
{
    public static class DeployAppClient
    {
        public static void DeployApp(string endpoint, string appName)
        {
            try
            {
                JsonServiceClient client = new JsonServiceClient(endpoint);

                Console.WriteLine("----> Compressing files");
                string content = CompressionHelper.CompressFolderToString(Environment.CurrentDirectory);

                Console.WriteLine("----> Uploading files");
                client.Post(new DeployAppRequest()
                {
                    AppName = appName,
                    Contents = content
                });

                Console.WriteLine("----> Restarting app");
            }
            catch (WebServiceException e)
            {
                Console.WriteLine(e.ResponseBody);
                Console.WriteLine(e.ErrorMessage);
                Console.WriteLine(e.ServerStackTrace);
            }
        }
    }
}

