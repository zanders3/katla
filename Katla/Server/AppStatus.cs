using System;
using System.Linq;
using System.Collections.Generic;

using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;

namespace zanders3.Katla.Server
{
    public class AppStatus
    {
        [AutoIncrement]
        public int ID { get; set; }

        [Index(Unique=true)]
        public string AppName { get; set; }

        public string InternalIP { get; set; }
        public string HostName { get; set; }
        public bool Running { get; set; }
    }

    public static class AppStatusModel
    {
        private static IDbConnectionFactory factory;

        public static void Setup(IDbConnectionFactory factory)
        {
            AppStatusModel.factory = factory;
            factory.OpenDbConnection().CreateTableIfNotExists(typeof(AppStatus));
        }

        public static List<AppStatus> Get()
        {
            return factory.OpenDbConnection().Select<AppStatus>();
        }

        public static AppStatus Get(string appName)
        {
            return factory.OpenDbConnection().Select<AppStatus>(app => app.AppName == appName).FirstOrDefault();
        }

        public static void Save(AppStatus status)
        {
            using (var conn = factory.OpenDbConnection())
            {
                if (conn.Select<AppStatus>(app => app.AppName == status.AppName).Count > 0)
                    conn.Update(status, app => app.ID == status.ID);
                else
                    conn.Insert(status);
            }
        }
    }

    [Route("/AppStatus")]
    [Route("/AppStatus/{AppName}")]
    public class AppStatusRequest : IReturn<List<AppStatus>>
    {
        public string AppName { get; set; }
    }

    public class AppStatusService : Service
    {
        public List<AppStatus> Get(AppStatusRequest request)
        {
            if (string.IsNullOrEmpty(request.AppName))
                return AppStatusModel.Get();
            else
                return new List<AppStatus>()
                {
                    AppStatusModel.Get(request.AppName)
                };
        }
    }
}

