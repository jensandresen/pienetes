using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pienetes.App.Application;
using Pienetes.App.Domain.Events;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.Persistence;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxManifestApplicationServiceDecorator : IManifestApplicationService
    {
        private readonly IManifestApplicationService _inner;
        private readonly IEventRegistry _eventRegistry;
        private readonly IMessageSerializer _serializer;
        private readonly PienetesDbContext _dbContext;

        public OutboxManifestApplicationServiceDecorator(IManifestApplicationService inner, 
            IEventRegistry eventRegistry, IMessageSerializer serializer,
            PienetesDbContext dbContext)
        {
            _inner = inner;
            _eventRegistry = eventRegistry;
            _serializer = serializer;
            _dbContext = dbContext;
        }

        public async Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType)
        {
            var innerResult = await _inner.QueueManifest(manifestContent, contentType);

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
            
            return innerResult;
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
    
    public class BackgroundMessageDispatcher : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BackgroundMessageDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // scope.ServiceProvider
                }
            }, stoppingToken);
        }
    }

    public class ExternalEventDispatcher
    {
        private readonly IEventRegistry _eventRegistry;

        public ExternalEventDispatcher(IEventRegistry eventRegistry)
        {
            _eventRegistry = eventRegistry;
            
            // not done!
        }
    }
}