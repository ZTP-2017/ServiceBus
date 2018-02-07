using System;

namespace Scheduler.Logger
{
    public interface ILoggerService
    {
        void CreateLog(LoggerService.LogType logType, string message, Exception exception);
    }
}
