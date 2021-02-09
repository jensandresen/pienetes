using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxScheduleImmediatelyManifestApplicationService : IManifestApplicationService
    {
        private readonly IManifestApplicationService _inner;
        private readonly IOutboxScheduler _outboxScheduler;

        public OutboxScheduleImmediatelyManifestApplicationService(IManifestApplicationService inner, IOutboxScheduler outboxScheduler)
        {
            _inner = inner;
            _outboxScheduler = outboxScheduler;
        }

        public async Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType)
        {
            var innerResult = await _inner.QueueManifest(manifestContent, contentType);
            _outboxScheduler.ScheduleNow();

            return innerResult;
        }
    }
}