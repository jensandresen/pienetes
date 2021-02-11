using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pienetes.App.Domain.Events;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class InboxDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public InboxDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Dispatch(InboxMessage message)
        {
            Console.WriteLine($"Inbox: Going to internally dispatch '{message.MessageType}'...");
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var eventRegistry = scope.ServiceProvider.GetRequiredService<IEventRegistry>();
                var serializer = scope.ServiceProvider.GetRequiredService<IMessageSerializer>();

                var registration = eventRegistry.FindRegistrationFor(message.MessageType);
                if (registration == null)
                {
                    throw new Exception($"Error! Event type \"{message.MessageType}\" has not been registered in the event registry.");
                }

                var handlerTypes = registration.EventHandlerInstanceTypes.ToArray();
                if (handlerTypes.Length == 0)
                {
                    throw new Exception($"Error! No event handlers has been registered for event type {registration.EventType} ({registration.EventInstanceType.FullName}).");
                }

                var domainEvent = serializer.Deserialize(message.Payload, registration.EventInstanceType);
                foreach (var handlerType in handlerTypes)
                {
                    Console.WriteLine($"Inbox: Internally handling message '{message.MessageType}' with handler '{handlerType.FullName}'...");
                    var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                    await DoHandleEvent((dynamic) domainEvent, (dynamic) handler);
                }
            }
        }
        
        private static async Task DoHandleEvent<TEvent>(TEvent domainEvent, IEventHandler<TEvent> handler) where TEvent : IDomainEvent
        {
            await handler.Handle(domainEvent);
        }
    }
}