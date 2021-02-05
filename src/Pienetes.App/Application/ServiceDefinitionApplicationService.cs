using System.Collections.Generic;
using System.Threading.Tasks;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Application
{
    public class ServiceDefinitionApplicationService : IServiceDefinitionApplicationService
    {
        private readonly IServiceDefinitionRepository _serviceDefinitionRepository;

        public ServiceDefinitionApplicationService(IServiceDefinitionRepository serviceDefinitionRepository)
        {
            _serviceDefinitionRepository = serviceDefinitionRepository;
        }
        
        public async Task AddOrUpdateService(string serviceName, ServiceImage image, IEnumerable<ServicePortMapping> ports, 
            IEnumerable<ServiceSecret> secrets, IEnumerable<ServiceEnvironmentVariable> environmentVariables)
        {
            var serviceId = ServiceId.Create(serviceName);
            var serviceDefinition = await _serviceDefinitionRepository.Get(serviceId);
            
            if (serviceDefinition == null)
            {
                serviceDefinition = ServiceDefinition.Create(serviceId, image);
                serviceDefinition.AddSecrets(secrets);
                serviceDefinition.AddPortMappings(ports);
                serviceDefinition.AddEnvironmentVariables(environmentVariables);
                await _serviceDefinitionRepository.Add(serviceDefinition);
            }
            else
            {
                serviceDefinition.Change(image, ports, secrets, environmentVariables);
            }
        }
    }
}