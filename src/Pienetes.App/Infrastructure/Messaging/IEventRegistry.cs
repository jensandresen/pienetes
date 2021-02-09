using Pienetes.App.Domain.Events;

namespace Pienetes.App.Infrastructure.Messaging
{
    public interface IEventRegistry
    {
        string GetEventTypeFor(IDomainEvent domainEvent);
        EventRegistry.Registration FindRegistrationFor(string eventType);
    }
}