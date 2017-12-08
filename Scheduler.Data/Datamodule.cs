using Autofac;
using Scheduler.Data.Interfaces;

namespace Scheduler.Data
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DataService>().As<IDataService>();
        }
    }
}
