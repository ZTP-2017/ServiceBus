using System;
using FluentMailer.Factory;
using MassTransit;
using Scheduler.Logger;
using Scheduler.Messaging;
using Scheduler.Receiver.Interfaces;
using Serilog;

namespace Scheduler.Receiver
{
    class Program
    {
        private static IMailService _mailService;
        private static ILoggerService _loggerService;

        static void Main(string[] args)
        {
            _loggerService = new LoggerService();

            var fluentMailer = FluentMailerFactory.Create();
            _mailService = new MailService(fluentMailer, _loggerService);

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

            _loggerService.CreateLog(LoggerService.LogType.Info, "Service bus started", null);

            Console.ReadLine();

            bus.Stop();

            _loggerService.CreateLog(LoggerService.LogType.Info, "Service bus stopped", null);
        }
    }
}
