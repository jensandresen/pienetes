using System;
using System.Threading.Tasks;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class TransactionalHelper
    {
        private readonly PienetesDbContext _dbContext;

        public TransactionalHelper(PienetesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<TResult> ExecuteInTransaction<TResult>(Func<Task<TResult>> func)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var innerResult = await func();
            
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

        public async Task ExecuteInTransaction(Func<Task> func)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await func();
            
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
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
}