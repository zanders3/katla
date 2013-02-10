using System;
using System.IO;
using ServiceStack.ServiceClient.Web;
using System.IO.Compression;
using zanders3.Katla.Server;
using System.Text;

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
                byte[] folder = CompressionHelper.CompressFolderToBytes(Environment.CurrentDirectory);

                Console.WriteLine("----> Uploading files (" + ((float)folder.Length / (1024.0f*1024.0f)) + " MB)");

                File.WriteAllBytes("deploy.gzip", folder);
                client.PostFile<int>("/API/Deploy/" + appName, new FileInfo("deploy.gzip"), "multipart/form-data");
                File.Delete("deploy.gzip");

                DeployAppStatusRequest request = new DeployAppStatusRequest() { AppName = appName };
                DeployAppStatusResponse response = client.Get(request);
                while (!response.Completed)
                {
                    Console.Write(response.Log);
                    response = client.Get(request);
                }
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

