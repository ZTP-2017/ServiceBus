using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
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
        private Timer _timer;
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

                if (messages.Count == 0)
                {
                    Log.Information("All messages send");
                    _endSending = true;

                    return;
                }

                _bus.Start();

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                foreach (var message in messages)
                {
                    var guid = Guid.NewGuid();

                    await _bus.Publish<IMessage>(new
                    {
                        Email = message.Email,
                        Subject = message.Subject,
                        Body = message.Body
                    });

                    Console.WriteLine($"Order {guid} sent");
                }

                _bus.Stop();
                stopWatch.Stop();

                _timer = new Timer(60000 - stopWatch.Elapsed.Milliseconds);
                _timer.Elapsed += Timer_Elapsed;
                _timer.AutoReset = true;
                _timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Messages send error: ");
            }
        }

        private List<Message> GetMessages()
        {
            var messages = _messages.Count() > 100 ?
                _messages.Take(100).ToList() :
                _messages;

            _messages = _messages.Where(x => messages.All(y => x != y)).ToList();

            return messages;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_endSending)
            {
                SendEmails();
            }
        }
    }
}
