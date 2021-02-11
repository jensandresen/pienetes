using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxScheduleImmediatelyManifestApplicationServiceDecorator : OutboxScheduleImmediatlyDecorator, IManifestApplicationService
    {
        private readonly IManifestApplicationService _inner;

        public OutboxScheduleImmediatelyManifestApplicationServiceDecorator(IManifestApplicationService inner, IOutboxScheduler outboxScheduler) : base(outboxScheduler)
        {
            _inner = inner;
        }

        public async Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType)
        {
            var innerResult = await _inner.QueueManifest(manifestContent, contentType);
            OutboxScheduler.ScheduleNow();

            return innerResult;
        }

        public async Task DequeueManifest(QueuedManifestId manifestId)
        {
            await _inner.DequeueManifest(manifestId);
            OutboxScheduler.ScheduleNow();
        }
    }
}