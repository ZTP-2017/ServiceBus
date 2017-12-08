using System.Collections.Generic;

namespace Scheduler.Data.Interfaces
{
    public interface IDataService
    {
        List<T> GetAllMessages<T>(string path);
    }
}
