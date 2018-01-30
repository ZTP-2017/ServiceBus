using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Scheduler.Data.Interfaces;
using Scheduler.Messaging;
using Scheduler.Sender.Interfaces;
using Scheduler.Sender.Models;
using Serilog;

namespace Scheduler.Sender
{
    public class Sender : ISender
    {
        private static int _skipMessagesCount;
        private static List<Message> _messages;

        private readonly IBusControl _bus;
        private readonly IDataService _dataService;

        public Sender(IDataService dataService)
        {
            _bus = GetBus();
            _dataService = dataService;
        }

        public void LoadAllMessagesFromFile(string path)
        {
            _messages = _dataService.GetAllMessages<Message>(path);
        }

        public void SendEmails()
        {
            try
            {
                Log.Information("Get data from file");
                var messages = GetMessages(100);

                messages.ForEach(async x =>
                {
                    await _bus.Publish<IMessage>(new Message
                    {
                        Email = x.Email,
                        Subject = x.Subject,
                        Body = x.Body
                    });
                    Log.Information($"Message {x.Subject} to {x.Email} was sent");
                });

                _bus.Start();

                Console.WriteLine("Bus started");

                Console.ReadLine();

                _bus.Stop();

                Console.WriteLine("Bus stoped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Messages send error: ");
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
