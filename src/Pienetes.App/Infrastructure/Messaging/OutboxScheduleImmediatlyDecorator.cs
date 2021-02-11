namespace Pienetes.App.Infrastructure.Messaging
{
    public abstract class OutboxScheduleImmediatlyDecorator
    {
        protected OutboxScheduleImmediatlyDecorator(IOutboxScheduler outboxScheduler)
        {
            OutboxScheduler = outboxScheduler;
        }
        
        public IOutboxScheduler OutboxScheduler { get; }
    }
}