using System;
using System.Linq;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace zanders3.Katla.Server
{
    [Route("/API/Deploy/{AppName}")]
    public class DeployAppRequest : IReturnVoid
    {
        public string AppName { get; set; }
    }

    [Route("/API/DeployStatus")]
    public class DeployAppStatusRequest : IReturn<DeployAppStatusResponse>
    {
        public string AppName { get; set; }
    }

    public class DeployAppStatusResponse
    {
        public string AppName { get; set; }
        public bool Completed { get; set; }
        public string Log { get; set; }

        public byte[] Contents;
        public StringBuilder Builder = new StringBuilder();
        public Exception Exception;
    }

    public class DeployAppService : Service
    {
        private static Dictionary<string, DeployAppStatusResponse> deployAppStatus = new Dictionary<string, DeployAppStatusResponse>();

        public void Post(DeployAppRequest request)
        {
            if (AppStatusModel.Get(request.AppName) == null)
                throw HttpError.NotFound("App not found: " + request.AppName);

            lock (deployAppStatus)
            {
                if (deployAppStatus.ContainsKey(request.AppName) && !deployAppStatus [request.AppName].Completed)
                    throw HttpError.Conflict("App deployment already in progress");
            }

            this.Request.Files[0].SaveTo("deploy.gzip");
            byte[] contents = File.ReadAllBytes("deploy.gzip");
            File.Delete("deploy.gzip");

            DeployAppStatusResponse response = new DeployAppStatusResponse()
            {
                AppName = request.AppName,
                Contents = contents,
                Completed = false,
                Log = string.Empty,
            };

            lock (deployAppStatus)
            {
                if (deployAppStatus.ContainsKey(request.AppName))
                    deployAppStatus.Remove(request.AppName);

                deployAppStatus.Add(request.AppName, response);
            }
            Task.Factory.StartNew(DeployAppProcess.Start, response);
        }

        public DeployAppStatusResponse Get(DeployAppStatusRequest request)
        {
            DeployAppStatusResponse response;
            lock (deployAppStatus)
            {
                if (!deployAppStatus.ContainsKey(request.AppName))
                    throw HttpError.NotFound("App deployment not in progress for " + request.AppName);

                response = deployAppStatus[request.AppName];
            }

            lock (response)
            {
                if (response.Exception != null)
                {
                    Exception e = response.Exception;
                    response.Exception = null;
                    throw e;
                }

                response.Log = response.Builder.ToString();
                response.Builder.Clear();
            }

            return response;
        }
    }
}

