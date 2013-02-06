using System;
using ServiceStack.ServiceClient.Web;
using System.IO;
using System.Threading;

namespace zanders3.Katla
{
    public static class CreateAppClient
    {
        public static void CreateApp(string endpoint, string appName, string buildScript)
        {
            JsonServiceClient client = new JsonServiceClient(endpoint);
            client.Post(new Server.CreateAppRequest()
            {
                AppName = appName,
                BuildScript = File.ReadAllText(buildScript)
            });

            try
            {
                Server.CreateAppStatus status;
                do
                {
                    status = client.Get(new Server.CreateAppStatusRequest() { AppName = appName });
                    if (status == null) break;
                    Console.Write(status.Log);
                    Thread.Sleep(200);
                }
                while (!status.Completed);
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

