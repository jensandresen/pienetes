using Pienetes.App.Domain.Model;

namespace Pienetes.App.Domain.Events
{
    public class NewServiceDefinitionAdded : IDomainEvent
    {
        public NewServiceDefinitionAdded(ServiceId serviceId)
        {
            ServiceId = serviceId;
        }
        
        public ServiceId ServiceId { get; }
    }

    public class ExistingServiceDefinitionHasBeenChanged : IDomainEvent
    {
        public ExistingServiceDefinitionHasBeenChanged(ServiceId serviceId)
        {
            ServiceId = serviceId;
        }
        
        public ServiceId ServiceId { get; }
    }
}