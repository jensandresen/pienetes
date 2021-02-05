using System.Collections.Generic;
using Pienetes.App.Domain.Events;

namespace Pienetes.App.Domain.Model
{
    public interface IAggregateEventsProvider
    {
        IEnumerable<IDomainEvent> GetRaisedEvents();
        void ClearRaisedEvents();
        string GetAggregateId();
    }
}