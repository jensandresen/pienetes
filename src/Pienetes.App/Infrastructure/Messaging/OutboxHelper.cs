using System;
using System.Linq;
using System.Threading.Tasks;
using Pienetes.App.Domain.Events;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.Persistence;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxHelper
    {
        private readonly IEventRegistry _eventRegistry;
        private readonly IMessageSerializer _serializer;
        private readonly PienetesDbContext _dbContext;

        public OutboxHelper(IEventRegistry eventRegistry, IMessageSerializer serializer, PienetesDbContext dbContext)
        {
            _eventRegistry = eventRegistry;
            _serializer = serializer;
            _dbContext = dbContext;
        }

        public async Task QueueRecentEvents()
        {
            var aggregates = _dbContext
                .ChangeTracker
                .Entries<IAggregateEventsProvider>()
                .Select(x => x.Entity)
                .ToArray();

            foreach (var aggregate in aggregates)
            {
                var domainEvents = aggregate.GetRaisedEvents();
                foreach (var domainEvent in domainEvents)
                {
                    var outboxMessage = CreateMessageFrom(domainEvent, aggregate);
                    await _dbContext.OutboxMessages.AddAsync(outboxMessage);
                }

                aggregate.ClearRaisedEvents();
            }
        }

        private OutboxMessage CreateMessageFrom(IDomainEvent domainEvent, IAggregateEventsProvider aggregate)
        {
            var eventType = _eventRegistry.GetEventTypeFor(domainEvent);
            var payload = _serializer.Serialize(domainEvent);

            return new OutboxMessage
            {
                MessageId = Guid.NewGuid().ToString("D"),
                MessageType = eventType,
                AggregateId = aggregate.GetAggregateId(),
                CreatedAt = DateTime.UtcNow,
                CustomHeaders = null,
                Payload = payload,
                SentAt = null,
            };
        }
    }
}