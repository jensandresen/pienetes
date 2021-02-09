using System;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxMessage
    {
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string AggregateId { get; set; }
        public string CustomHeaders { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}