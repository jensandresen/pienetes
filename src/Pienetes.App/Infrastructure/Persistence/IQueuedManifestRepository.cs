using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.WebApi;

namespace Pienetes.App.Infrastructure.Persistence
{
    public interface IQueuedManifestRepository
    {
        Task Add(QueuedManifest manifest);
        Task<QueuedManifest> Get(QueuedManifestId id);
        Task Remove(QueuedManifestId id);
    }
    
    public class QueuedManifestRepository : IQueuedManifestRepository
    {
        private readonly PienetesDbContext _dbContext;

        public QueuedManifestRepository(PienetesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task Add(QueuedManifest manifest)
        {
            await _dbContext.QueuedManifests.AddAsync(manifest);
        }

        public async Task<QueuedManifest> Get(QueuedManifestId id)
        {
            string db_id = id;
            return await _dbContext.QueuedManifests.SingleOrDefaultAsync(x => x.Id == db_id);
        }

        public async Task Remove(QueuedManifestId id)
        {
            var manifest = await Get(id);
            if (manifest != null)
            {
                _dbContext.QueuedManifests.Remove(manifest);
            }
        }
    }
}