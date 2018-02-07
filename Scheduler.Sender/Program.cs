using System;
using System.Configuration;
using Autofac;
using Hangfire;
using Scheduler.Data;
using Scheduler.Logger;
using Scheduler.Sender.Interfaces;
using Topshelf;
using Topshelf.Autofac;

namespace Scheduler.Sender
{
    class Program
    {
        public static IContainer Container { get; set; }

        static void Main(string[] args)
        {
            ConfigureAutofac();
            ConfigureHangfire();
            ConfigureTopshelf();

            Console.ReadKey();
        }

        private static void ConfigureAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Scheduler>();
            
            builder.RegisterModule<DataModule>();
            builder.RegisterModule<LoggerModule>();

            builder.RegisterType<Sender>();
            builder.RegisterType<Sender>().As<ISender>();

            builder.RegisterInstance(GetAppSettings());

            Container = builder.Build();
        }

        private static void ConfigureHangfire()
        {
            GlobalConfiguration.Configuration.UseAutofacActivator(Container);
        }

        private static void ConfigureTopshelf()
        {
            HostFactory.Run(configure =>
            {
                configure.UseAutofacContainer(Container);
                configure.Service<Scheduler>(service =>
                {
                    service.ConstructUsingAutofacContainer();
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                
                configure.SetServiceName("MailerService");
                configure.SetDisplayName("Mailer Service");
                configure.SetDescription("Mailer service ZTP");
                configure.RunAsLocalService();
            });
        }

        private static Settings GetAppSettings()
        {
            return new Settings
            {
                HostingUrl = ConfigurationManager.AppSettings["hostingUrl"],
                DataFilePath = ConfigurationManager.AppSettings["messagesFilePath"]
            };
        }
    }
}
