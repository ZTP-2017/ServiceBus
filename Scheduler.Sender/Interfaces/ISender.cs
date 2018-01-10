namespace Scheduler.Sender.Interfaces
{
    public interface ISender
    {
        void SendEmails();
        void SetSkipValue(int value);
    }
}
