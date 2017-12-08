using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Scheduler.Data.Interfaces;

namespace Scheduler.Data
{
    public class DataService : IDataService
    {
        public List<T> GetAllMessages<T>(string path)
        {
            using (var sr = new StreamReader(path))
            {
                var csvReader = new CsvReader(sr);
                return csvReader.GetRecords<T>().ToList();
            }
        }
    }
}
