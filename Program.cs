using Topshelf;

namespace ServiceGuard
{
    internal class Program
    {
        public static StartService StartService { get; private set; } = null;
        static void Main(string[] args)
        {
            StartService = new StartService(args);
            HostFactory.Run(x =>
            {
                x.Service<StartService>(s =>
                {
                    s.ConstructUsing(name => StartService);
                    s.WhenStarted(svc => svc.OnStart());
                    s.WhenStopped(svc => svc.OnStop());
                });
                x.StartAutomaticallyDelayed();
                x.RunAsLocalSystem();
                x.SetDescription("Сервис обработки данных Guard");
                x.SetDisplayName("Guard Data Processing Service");
                x.SetServiceName("Guard Service");
            });
        }
    }
}
