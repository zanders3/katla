using System;
using ServiceStack.WebHost.Endpoints;

namespace zanders3.Katla.Server
{
    public class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("Katla API Server", typeof(AppHost).Assembly)
        {
        }
        
        public override void Configure(Funq.Container container)
        {
            Config.DebugMode = true;
        }
    }
    
    public static class KatlaServer
    {
        public static void Start(string endpoint)
        {
            AppHost appHost = new AppHost();
            appHost.Init();

            string server = endpoint;
            appHost.Start(server);
            
            Console.WriteLine("Server started. Listening on " + server);
            while (true) System.Threading.Thread.Yield();
        }
    }
}

