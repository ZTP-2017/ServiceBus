using System;
using Hangfire;
using Microsoft.Owin.Hosting;
using Scheduler.Logger;
using Scheduler.Sender.Interfaces;

namespace Scheduler.Sender
{
    public class Scheduler
    {
        private readonly ISender _sender;
        private readonly ILoggerService _loggerService;
        private readonly Settings _settings;

        public Scheduler(ISender sender, ILoggerService loggerService, Settings settings)
        {
            _sender = sender;
            _settings = settings;
            _loggerService = loggerService;
        }

        private IDisposable _webApp;

        public void Start()
        {
            try
            {
                _webApp = WebApp.Start<Startup>(_settings.HostingUrl);

                _sender.SetSkipValue(0);
                _sender.LoadAllMessagesFromFile(_settings.DataFilePath);

                RecurringJob.AddOrUpdate(
                    () => _sender.SendEmails(),
                    Cron.Minutely
                );

                _loggerService.CreateLog(LoggerService.LogType.Info, "Start service", null);
            }
            catch (Exception ex)
            {
                _loggerService.CreateLog(LoggerService.LogType.Error, "Start service error", ex);
            }
        }

        public void Stop()
        {
            try
            {
                _webApp.Dispose();
                _loggerService.CreateLog(LoggerService.LogType.Info, "Stop service", null);
            }
            catch (Exception ex)
            {
                _loggerService.CreateLog(LoggerService.LogType.Error, "Stop service error", ex);
            }
        }
    }
}
