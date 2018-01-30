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

        public void Start(Settings settings)
        {
            try
            {
                _webApp = WebApp.Start<Startup>(settings.HostingUrl);

                _sender.SetSkipValue(0);
                _sender.LoadAllMessagesFromFile(settings.DataFilePath);

                RecurringJob.AddOrUpdate(
                    () => _sender.SendEmails(),
                    Cron.Minutely
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
