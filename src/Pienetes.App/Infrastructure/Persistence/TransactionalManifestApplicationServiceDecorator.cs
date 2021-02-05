using System;
using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.Messaging;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class TransactionalManifestApplicationServiceDecorator : IManifestApplicationService
    {
        private readonly IManifestApplicationService _inner;
        private readonly PienetesDbContext _dbContext;

        public TransactionalManifestApplicationServiceDecorator(IManifestApplicationService inner, PienetesDbContext dbContext)
        {
            _inner = inner;
            _dbContext = dbContext;
        }

        public async Task<QueuedManifestId> QueueManifest(string manifestContent, string contentType)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var innerResult = await _inner.QueueManifest(manifestContent, contentType);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return innerResult;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                    await transaction.RollbackAsync();

                    throw;
                }
            }
        }
    }
    
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