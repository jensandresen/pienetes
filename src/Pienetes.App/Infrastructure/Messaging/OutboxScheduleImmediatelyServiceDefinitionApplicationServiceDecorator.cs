using System.Collections.Generic;
using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxScheduleImmediatelyServiceDefinitionApplicationServiceDecorator : OutboxScheduleImmediatlyDecorator, IServiceDefinitionApplicationService
    {
        private readonly IServiceDefinitionApplicationService _inner;

        public OutboxScheduleImmediatelyServiceDefinitionApplicationServiceDecorator(IOutboxScheduler outboxScheduler, IServiceDefinitionApplicationService inner) : base(outboxScheduler)
        {
            _inner = inner;
        }

        public async Task AddOrUpdateService(string serviceName, ServiceImage image, IEnumerable<ServicePortMapping> ports, IEnumerable<ServiceSecret> secrets,
            IEnumerable<ServiceEnvironmentVariable> environmentVariables)
        {
            await _inner.AddOrUpdateService(serviceName, image, ports, secrets, environmentVariables);
            OutboxScheduler.ScheduleNow();
        }
    }
}