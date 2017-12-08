using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Scheduler.Sender.Startup))]
namespace Scheduler.Sender
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();
            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
