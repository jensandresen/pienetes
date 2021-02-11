using System.Threading.Tasks;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.Persistence;

namespace Pienetes.App.Application
{
    public class ManifestApplicationService : IManifestApplicationService
    {
        private readonly IQueuedManifestRepository _queuedManifestRepository;

        public ManifestApplicationService(IQueuedManifestRepository queuedManifestRepository)
        {
            _queuedManifestRepository = queuedManifestRepository;
        }
        
        public async Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType)
        {
            var manifest = QueuedManifest.Create(manifestContent, contentType);
            await _queuedManifestRepository.Add(manifest);

            return manifest.Id;
        }

        public async Task DequeueManifest(QueuedManifestId manifestId)
        {
            await _queuedManifestRepository.Remove(manifestId);
        }
    }
}