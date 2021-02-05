using System.Collections.Generic;
using Pienetes.App.Domain.Events;

namespace Pienetes.App.Domain.Model
{
    public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateEventsProvider
    {
        private readonly LinkedList<IDomainEvent> _raisedEvents = new LinkedList<IDomainEvent>();

        protected AggregateRoot()
        {
            
        }
        
        protected AggregateRoot(TId id) : base(id)
        {
            
        }

        protected void Raise(IDomainEvent @event)
        {
            _raisedEvents.AddLast(@event);
        }

        public IEnumerable<IDomainEvent> GetRaisedEvents()
        {
            return _raisedEvents;
        }

        public void ClearRaisedEvents()
        {
            _raisedEvents.Clear();
        }

        public string GetAggregateId() => Id.ToString();
    }
}