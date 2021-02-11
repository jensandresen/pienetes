namespace Pienetes.App.Infrastructure.Persistence
{
    public abstract class TransactionalDecorator
    {
        protected TransactionalDecorator(TransactionalHelper transactionalHelper)
        {
            TransactionalHelper = transactionalHelper;
        }

        protected TransactionalHelper TransactionalHelper { get; }
    }
}