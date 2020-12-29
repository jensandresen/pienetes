using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pienetes.App.Domain;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class ServiceDefinitionRepository : IServiceDefinitionRepository
    {
        private readonly PienetesDbContext _dbContext;

        public ServiceDefinitionRepository(PienetesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<IEnumerable<ServiceDefinition>> GetAll()
        {
            return Enumerable.Empty<ServiceDefinition>();
        }

        public async Task<ServiceDefinition> FindByChecksum(string checksum)
        {
            return null;
        }

        public async Task<ServiceDefinition> FindByName(string name)
        {
            return null;
        }
    }
}