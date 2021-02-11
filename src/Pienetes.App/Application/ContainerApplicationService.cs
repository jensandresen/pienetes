using System;
using System.Threading.Tasks;
using Pienetes.App.Domain.Model;
using Pienetes.App.Domain.Services;

namespace Pienetes.App.Application
{
    public class ContainerApplicationService : IContainerApplicationService
    {
        private readonly IServiceDefinitionRepository _serviceDefinitionRepository;
        private readonly IContainerDomainService _containerDomainService;

        public ContainerApplicationService(IServiceDefinitionRepository serviceDefinitionRepository, 
            IContainerDomainService containerDomainService)
        {
            _serviceDefinitionRepository = serviceDefinitionRepository;
            _containerDomainService = containerDomainService;
        }
        
        public async Task SpinUpNewContainer(ServiceId serviceId)
        {
            var serviceDefinition = await _serviceDefinitionRepository.Get(serviceId);
            if (serviceDefinition == null)
            {
                throw new Exception($"Error! Service definition with id {serviceId} could not be found!");
            }

            await _containerDomainService.PullContainerImage(serviceDefinition.Image);
            await _containerDomainService.RemoveContainer(serviceDefinition.Id);
            await _containerDomainService.CreateNewContainerFrom(serviceDefinition);
            await _containerDomainService.StartContainer(serviceDefinition.Id);
        }

        public async Task UpdateExistingContainer(ServiceId serviceId)
        {
            var serviceDefinition = await _serviceDefinitionRepository.Get(serviceId);
            if (serviceDefinition == null)
            {
                throw new Exception($"Error! Service definition with id {serviceId} could not be found!");
            }

            await _containerDomainService.PullContainerImage(serviceDefinition.Image);

            var tempContainerName = string.Join("-", serviceDefinition.Id, "old");
            await _containerDomainService.RenameContainer(serviceDefinition.Id, tempContainerName);
            
            await _containerDomainService.CreateNewContainerFrom(serviceDefinition);
            await _containerDomainService.StopContainer(tempContainerName);
            await _containerDomainService.StartContainer(serviceDefinition.Id);
            await _containerDomainService.RemoveContainer(tempContainerName);
        }
    }
}