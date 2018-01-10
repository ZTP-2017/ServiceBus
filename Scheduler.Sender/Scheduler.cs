using System;
using Hangfire;
using Microsoft.Owin.Hosting;
using Scheduler.Sender.Interfaces;
using Serilog;

namespace Scheduler.Sender
{
    public class Scheduler
    {
        private readonly ISender _sender;

        public Scheduler(ISender sender)
        {
            _sender = sender;
        }

        private IDisposable _webApp;

        public void Start()
        {
            try
            {
                _webApp = WebApp.Start<Startup>("http://localhost:8080");

                _sender.SetSkipValue(0);

                RecurringJob.AddOrUpdate(
                    () => _sender.SendEmails(),
                    Cron.Daily
                );

                Log.Information("Start service");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Start service error: ");
            }
        }

        public void Stop()
        {
            try
            {
                _webApp.Dispose();
                Log.Information("Stop service");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Stop service error: ");
            }
        }
    }
}
