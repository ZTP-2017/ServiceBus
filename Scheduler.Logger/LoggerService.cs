using System;
using Serilog;

namespace Scheduler.Logger
{
    public class LoggerService : ILoggerService
    {
        public enum LogType { Info, Error, Warning }

        public LoggerService()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile("log-{Date}.txt")
                .WriteTo.ColoredConsole()
                .CreateLogger();
        }

        public void CreateLog(LogType logType, string message, Exception exception)
        {
            switch (logType)
            {
                case LogType.Error:
                    Log.Error(exception, message);
                    break;
                case LogType.Warning:
                    Log.Warning(exception, message);
                    break;
                case LogType.Info:
                    Log.Information(exception, message);
                    break; ;
            }
        }
    }
}
