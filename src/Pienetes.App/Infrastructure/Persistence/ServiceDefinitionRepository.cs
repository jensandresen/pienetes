using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class ServiceDefinitionRepository : IServiceDefinitionRepository
    {
        private readonly PienetesDbContext _dbContext;

        public ServiceDefinitionRepository(PienetesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceDefinition> Get(ServiceId id)
        {
            string serviceId = id;
            return await _dbContext.ServiceDefinitions.SingleOrDefaultAsync(x => x.Id == serviceId);
        }

        public async Task<IEnumerable<ServiceDefinition>> GetAll()
        {
            return await _dbContext.ServiceDefinitions.OrderBy(x => x.Id).ToListAsync();
        }

        public async Task Add(ServiceDefinition serviceDefinition)
        {
            await _dbContext.ServiceDefinitions.AddAsync(serviceDefinition);
        }

        public async Task<bool> Exists(ServiceId serviceId)
        {
            string id = serviceId;
            return await _dbContext.ServiceDefinitions.AnyAsync(x => x.Id == id);
        }
    }
}