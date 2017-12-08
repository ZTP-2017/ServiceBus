namespace Scheduler.Messaging
{
    public interface IMessage
    {
        string Email { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
    }
}
