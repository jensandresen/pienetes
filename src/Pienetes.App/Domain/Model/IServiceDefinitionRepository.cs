using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pienetes.App.Domain.Model
{
    public interface IServiceDefinitionRepository
    {
        Task<ServiceDefinition> Get(ServiceId id);
        Task<IEnumerable<ServiceDefinition>> GetAll();
        Task Add(ServiceDefinition serviceDefinition);
        Task<bool> Exists(ServiceId serviceId);
    }
}