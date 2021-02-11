using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Events;

namespace Pienetes.App.Domain.Policies
{
    public class SpinUpContainerForService : 
        IEventHandler<NewServiceDefinitionAdded>,
        IEventHandler<ExistingServiceDefinitionHasBeenChanged>
    {
        private readonly IContainerApplicationService _containerApplicationService;

        public SpinUpContainerForService(IContainerApplicationService containerApplicationService)
        {
            _containerApplicationService = containerApplicationService;
        }
        public async Task Handle(NewServiceDefinitionAdded e)
        {
            await _containerApplicationService.SpinUpNewContainer(e.ServiceId);
        }

        public async Task Handle(ExistingServiceDefinitionHasBeenChanged e)
        {
            await _containerApplicationService.UpdateExistingContainer(e.ServiceId);
        }
    }
}