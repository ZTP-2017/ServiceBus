using System;
using System.Collections.Generic;
using System.Linq;
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
        private static int skipMessagesCount;
        private List<Message> _messages;
        private readonly IBusControl _bus;
        private bool _endSending;

        public Sender(IDataService dataService)
        {
            _endSending = false;
            _messages = dataService.GetAllMessages<Message>(Settings.DataFilePath);

            _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.PublisherConfirmation = false;

                var host = cfg.Host(new Uri("rabbitmq://localhost/"), hostCfg =>
                {
                    hostCfg.Username("guest");
                    hostCfg.Password("guest");
                });
            });
        }

        public async void SendEmails()
        {
            try
            {
                Log.Information("Get data from file");
                var messages = GetMessages();

                _bus.Start();

                messages.ForEach(async x =>
                {
                    await _bus.Publish<IMessage>(new
                    {
                        Email = x.Email,
                        Subject = x.Subject,
                        Body = x.Body
                    });
                    Log.Information($"Message {x.Subject} to {x.Email} was sent");
                });

                _bus.Stop();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Messages send error: ");
            }
        }

        public void SetSkipValue(int value)
        {
            skipMessagesCount = value;
        }

        private List<Message> GetMessages()
        {
            var messages = _messages.Skip(skipMessagesCount).Take(100).ToList();

            skipMessagesCount += messages.Count;

            return messages;
        }
    }
}
