using System.Collections.Generic;
using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class TransactionalServiceDefinitionApplicationServiceDecorator : TransactionalDecorator, IServiceDefinitionApplicationService
    {
        private readonly IServiceDefinitionApplicationService _inner;

        public TransactionalServiceDefinitionApplicationServiceDecorator(IServiceDefinitionApplicationService inner,
            TransactionalHelper transactionalHelper) : base(transactionalHelper)
        {
            _inner = inner;
        }

        public Task AddOrUpdateService(string serviceName, ServiceImage image, IEnumerable<ServicePortMapping> ports, 
            IEnumerable<ServiceSecret> secrets, IEnumerable<ServiceEnvironmentVariable> environmentVariables)
        {
            return TransactionalHelper.ExecuteInTransaction(() => _inner.AddOrUpdateService(serviceName, image, ports, secrets, environmentVariables));
        }

    }
}