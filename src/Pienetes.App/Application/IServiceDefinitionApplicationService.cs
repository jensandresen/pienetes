using System.Collections.Generic;
using System.Threading.Tasks;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Application
{
    public interface IServiceDefinitionApplicationService
    {
        Task AddOrUpdateService(string serviceName, ServiceImage image, IEnumerable<ServicePortMapping> ports, 
            IEnumerable<ServiceSecret> secrets, IEnumerable<ServiceEnvironmentVariable> environmentVariables);
    }
}