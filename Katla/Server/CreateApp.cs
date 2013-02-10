using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using ServiceStack.Common.Web;
using System.Text;
using System.Threading;

using ServiceStack.OrmLite;

namespace zanders3.Katla.Server
{
    [Route("/CreateApp")]
    public class CreateAppRequest : IReturnVoid
    {
        public string AppName { get; set; }
    }

    [Route("/CreateApp/{AppName}")]
    public class CreateAppStatusRequest : IReturn<CreateAppStatus>
    {
        public string AppName { get; set; }
    }

    public class CreateAppStatus
    {
        public StringBuilder Builder = new StringBuilder();

        public bool Completed { get; set; }
        public string Log { get; set; }
    }
    
    public class CreateAppService : Service
    {
        static Dictionary<string, CreateAppStatus> appCreationStatus = new Dictionary<string, CreateAppStatus>();
        static Exception exceptionToThrow = null;

        public void Post(CreateAppRequest request)
        {
            if (string.IsNullOrEmpty(request.AppName))
                throw HttpError.Conflict("Missing AppName");
            if (Directory.Exists("/var/lib/lxc/" + request.AppName))
                throw HttpError.Conflict("App already exists");

            lock (appCreationStatus)
            {
                appCreationStatus.Remove(request.AppName);
                appCreationStatus.Add(request.AppName, new CreateAppStatus());
            }

            Task.Factory.StartNew(CreateApp, request);
        }

        public CreateAppStatus Get(CreateAppStatusRequest request)
        {
            if (exceptionToThrow != null)
            {
                Exception e = exceptionToThrow;
                exceptionToThrow = null;
                throw e;
            }

            lock (appCreationStatus)
            {
                if (appCreationStatus.ContainsKey(request.AppName))
                {
                    CreateAppStatus status = appCreationStatus[request.AppName];
                    status.Log = status.Builder.ToString();
                    status.Builder.Clear();
                    return status;
                }
                else
                    throw HttpError.NotFound("No app creation in progress for: " + request.AppName);
            }
        }

        static void CreateApp(object state)
        {
            CreateAppRequest request = (CreateAppRequest)state;
            Action<string> logMessage = message =>
            {
                lock (appCreationStatus)
                {
                    appCreationStatus[request.AppName].Builder.AppendLine(message);
                }
            };

            try
            {
                CreateAppProcess.CreateApp(request.AppName, logMessage);
                lock (appCreationStatus)
                {
                    appCreationStatus[request.AppName].Completed = true;
                }
            } 
            catch (Exception e)
            {
                exceptionToThrow = e;
            }
        }
    }
}

