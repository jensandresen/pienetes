namespace Pienetes.App.Infrastructure.Messaging
{
    public class InboxMessage
    {
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string Payload { get; set; }
        public string Format { get; set; }
    }
}