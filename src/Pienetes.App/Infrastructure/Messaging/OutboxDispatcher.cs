using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pienetes.App.Infrastructure.Persistence;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public OutboxDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        private async Task<IEnumerable<OutboxMessage>> GetPendingMessages()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PienetesDbContext>();
                
                return await dbContext.OutboxMessages
                    .Where(x => x.SentAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .ToListAsync();
            }
        }

        public async Task DispatchPendingMessages()
        {
            var pendingMessages = await GetPendingMessages();

            foreach (var message in pendingMessages)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var messagingGateway = scope.ServiceProvider.GetRequiredService<MessagingGateway>();
                    var eventRegistry = scope.ServiceProvider.GetRequiredService<IEventRegistry>();
                    
                    var registration = eventRegistry.FindRegistrationFor(message.MessageType);
                    if (registration == null)
                    {
                        throw new Exception($"Error! Event type \"{message.MessageType}\" has not been registered in the event registry.");
                    }

                    var dbContext = scope.ServiceProvider.GetRequiredService<PienetesDbContext>();
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            dbContext.Attach(message);
                            message.SentAt = DateTime.Now;

                            await messagingGateway.Publish(message.MessageId, message.MessageType, message.Payload);
                            
                            await dbContext.SaveChangesAsync();
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
    }
}