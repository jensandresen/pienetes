using System.Collections.Generic;
using Pienetes.App.Domain.Events;

namespace Pienetes.App.Infrastructure.Messaging
{
    public interface IEventRegistry
    {
        string GetEventTypeFor(IDomainEvent domainEvent);
        List<EventRegistry.Registration> Registrations { get; }
        EventRegistry.Registration FindRegistrationFor(string eventType);
    }
}