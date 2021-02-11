using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class TransactionalManifestApplicationServiceDecorator : TransactionalDecorator, IManifestApplicationService
    {
        private readonly IManifestApplicationService _inner;

        public TransactionalManifestApplicationServiceDecorator(IManifestApplicationService inner, TransactionalHelper transactionalHelper) : base(transactionalHelper)
        {
            _inner = inner;
        }

        public Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType)
        {
            return TransactionalHelper.ExecuteInTransaction(() => _inner.QueueManifest(manifestContent, contentType));
        }

        public Task DequeueManifest(QueuedManifestId manifestId)
        {
            return TransactionalHelper.ExecuteInTransaction(() => _inner.DequeueManifest(manifestId));
        }
    }
}