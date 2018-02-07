using Autofac;

namespace Scheduler.Logger
{
    public class LoggerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LoggerService>().As<ILoggerService>();
        }
    }
}
