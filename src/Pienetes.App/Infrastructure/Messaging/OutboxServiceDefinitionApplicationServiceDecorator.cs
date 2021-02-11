using System.Collections.Generic;
using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxServiceDefinitionApplicationServiceDecorator : OutboxDecorator, IServiceDefinitionApplicationService
    {
        private readonly IServiceDefinitionApplicationService _inner;

        public OutboxServiceDefinitionApplicationServiceDecorator(IServiceDefinitionApplicationService inner, OutboxHelper outboxHelper) : base(outboxHelper)
        {
            _inner = inner;
        }

        public async Task AddOrUpdateService(string serviceName, ServiceImage image, IEnumerable<ServicePortMapping> ports, IEnumerable<ServiceSecret> secrets,
            IEnumerable<ServiceEnvironmentVariable> environmentVariables)
        {
            await _inner.AddOrUpdateService(serviceName, image, ports, secrets, environmentVariables);
            await OutboxHelper.QueueRecentEvents();
        }
    }
}