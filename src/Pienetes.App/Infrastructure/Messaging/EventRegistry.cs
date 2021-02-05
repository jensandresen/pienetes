using System;
using System.Collections.Generic;
using System.Linq;
using Pienetes.App.Domain.Events;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class EventRegistry : IEventRegistry
    {
        private readonly List<Registration> _registrations = new List<Registration>();
        
        public string GetEventTypeFor(IDomainEvent domainEvent)
        {
            var instanceType = domainEvent.GetType();
            var registration = _registrations.SingleOrDefault(x => x.EventInstanceType == instanceType);
            
            if (registration == null)
            {
                throw new Exception($"nooooo! no registration for instance type {instanceType.FullName}");
                // return null;
            }

            return registration.EventType;
        }

        public Registration FindRegistrationFor(string eventType)
        {
            return _registrations.SingleOrDefault(x => x.EventType == eventType);
        }
        
        public record Registration(string EventType, Type EventInstanceType, IEnumerable<Type> EventHandlerInstanceTypes);

        public List<Registration> Registrations => _registrations;

        public EventRegistry Register<TEvent>(string eventType, Action<HandlerConfigurator<TEvent>> handlers = null)
            where TEvent : IDomainEvent
        {
            var handlerConfigurator = new HandlerConfigurator<TEvent>();
            handlers?.Invoke(handlerConfigurator);

            _registrations.Add(new Registration(
                EventType: eventType,
                EventInstanceType: typeof(TEvent),
                EventHandlerInstanceTypes: handlerConfigurator.RegisteredHandlerTypes)
            );

            return this;
        }

        public class HandlerConfigurator<TEvent> where TEvent : IDomainEvent
        {
            private readonly List<Type> _handlerTypes = new List<Type>();
            
            public HandlerConfigurator<TEvent> Add<THandler>() where THandler : class, IEventHandler<TEvent>
            {
                _handlerTypes.Add(typeof(THandler));
                return this;
            }

            public IEnumerable<Type> RegisteredHandlerTypes => _handlerTypes;
        }
    }
}