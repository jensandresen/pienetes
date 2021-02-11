namespace Pienetes.App.Infrastructure.Messaging
{
    public abstract class OutboxDecorator
    {
        protected readonly OutboxHelper OutboxHelper;

        protected OutboxDecorator(OutboxHelper outboxHelper)
        {
            OutboxHelper = outboxHelper;
        }
    }
}