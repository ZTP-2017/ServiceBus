using System.Threading.Tasks;

namespace Scheduler.Receiver.Interfaces
{
    public interface IMailService
    {
        Task SendEmail(string email, string body, string subject);
    }
}
