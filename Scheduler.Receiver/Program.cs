using System;
using FluentMailer.Factory;
using MassTransit;
using Scheduler.Messaging;
using Scheduler.Receiver.Interfaces;
using Serilog;

namespace Scheduler.Receiver
{
    class Program
    {
        private static IMailService _mailService;

        static void Main(string[] args)
        {
            var fluentMailer = FluentMailerFactory.Create();
            _mailService = new MailService(fluentMailer);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.UseSerilog(logger);

                var host = cfg.Host(new Uri("rabbitmq://localhost/"), hostCfg =>
                {
                    hostCfg.Username("guest");
                    hostCfg.Password("guest");
                });

                cfg.ReceiveEndpoint(host, "order.service", e =>
                {
                    e.Handler<IMessage>(x => _mailService.SendEmail(x.Message.Email, x.Message.Body, x.Message.Subject));
                });
            });

            bus.Start();

            Console.WriteLine("Bus started");

            Console.ReadLine();

            bus.Stop();

            Console.WriteLine("Bus stoped");
        }
    }
}
