using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxManifestApplicationServiceDecorator : OutboxDecorator, IManifestApplicationService
    {
        private readonly IManifestApplicationService _inner;

        public OutboxManifestApplicationServiceDecorator(IManifestApplicationService inner, OutboxHelper outboxHelper) : base(outboxHelper)
        {
            _inner = inner;
        }

        public async Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType)
        {
            var innerResult = await _inner.QueueManifest(manifestContent, contentType);
            await OutboxHelper.QueueRecentEvents();

            return innerResult;
        }

        public async Task DequeueManifest(QueuedManifestId manifestId)
        {
            await _inner.DequeueManifest(manifestId);
            await OutboxHelper.QueueRecentEvents();
        }
    }
}