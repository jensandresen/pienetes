using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pienetes.App.Domain
{
    public interface IServiceDefinitionRepository
    {
        Task<IEnumerable<ServiceDefinition>> GetAll();
        Task<ServiceDefinition> FindByChecksum(string checksum);
        Task<ServiceDefinition> FindByName(string name);
    }
}