using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using Scheduler.Data.Interfaces;
using Scheduler.Logger;
using Scheduler.Messaging;
using Scheduler.Sender.Interfaces;
using Scheduler.Sender.Models;

namespace Scheduler.Sender
{
    public class Sender : ISender
    {
        private static int _skipMessagesCount;
        private static List<Message> _messages;

        private readonly IBusControl _bus;
        private readonly IDataService _dataService;
        private readonly ILoggerService _loggerService;

        public Sender(IDataService dataService, ILoggerService loggerService)
        {
            _bus = GetBus();
            _dataService = dataService;
            _loggerService = loggerService;
        }

        public void LoadAllMessagesFromFile(string path)
        {
            _messages = _dataService.GetAllMessages<Message>(path);
        }

        public void SendEmails()
        {
            try
            {
                _loggerService.CreateLog(LoggerService.LogType.Info, "Get data from file", null);
                var messages = GetMessages(100);

                messages.ForEach(async x =>
                {
                    await _bus.Publish<IMessage>(new Message
                    {
                        Email = x.Email,
                        Subject = x.Subject,
                        Body = x.Body
                    });
                });

                _bus.Start();

                _loggerService.CreateLog(LoggerService.LogType.Info, "Service bus started", null);

                Console.ReadLine();

                _bus.Stop();

                _loggerService.CreateLog(LoggerService.LogType.Info, "Service bus stopped", null);
                Console.WriteLine("Bus stoped");
            }
            catch (Exception ex)
            {
                _loggerService.CreateLog(LoggerService.LogType.Error, "Messages send error", ex);
            }
        }

        public void SetSkipValue(int value)
        {
            _skipMessagesCount = value;
        }

        private List<Message> GetMessages(int count)
        {
            var messages = _messages.Skip(_skipMessagesCount).Take(count).ToList();

            _skipMessagesCount += messages.Count;

            return messages;
        }

        private IBusControl GetBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.PublisherConfirmation = false;

                var host = cfg.Host(new Uri("rabbitmq://localhost/"), hostCfg =>
                {
                    hostCfg.Username("guest");
                    hostCfg.Password("guest");
                });
            });
        }
    }
}
