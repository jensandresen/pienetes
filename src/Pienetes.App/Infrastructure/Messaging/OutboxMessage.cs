using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pienetes.App.Domain.Events;
using Pienetes.App.Infrastructure.Persistence;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxMessage
    {
        public string MessageId { get; set; }
        public string MessageType { get; set; }
        public string AggregateId { get; set; }
        public string CustomHeaders { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }

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

        private async Task DoHandleEvent<TEvent>(TEvent domainEvent, IEventHandler<TEvent> handler) where TEvent : IDomainEvent
        {
            await handler.Handle(domainEvent);
        }
        
        public async Task DispatchPendingMessages()
        {
            var pendingMessages = await GetPendingMessages();

            foreach (var message in pendingMessages)
            {
                Console.WriteLine($"Dispatching message '{message.MessageType}'...");
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var eventRegistry = scope.ServiceProvider.GetRequiredService<IEventRegistry>();
                    var serializer = scope.ServiceProvider.GetRequiredService<IMessageSerializer>();

                    object CreateEventHandler(Type handlerType)
                    {
                        return scope.ServiceProvider.GetRequiredService(handlerType);
                    }

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

                            var domainEvent = serializer.Deserialize(message.Payload, registration.EventInstanceType);
                            
                            foreach (var handlerType in registration.EventHandlerInstanceTypes)
                            {
                                var handler = CreateEventHandler(handlerType);
                                Console.WriteLine($"Handling message '{message.MessageType}' with '{handler.GetType().FullName}'...");
                                await DoHandleEvent((dynamic) domainEvent, (dynamic) handler);
                            }
                            
                            await dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();

                            Console.WriteLine($"Successful dispatch of message '{message.MessageType}'!!!!");
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

    public interface IOutboxScheduler
    {
        void ScheduleNow();
    }
    
    public class OutboxScheduler : IHostedService, IDisposable, IOutboxScheduler
    {
        private static readonly AutoResetEvent ResetEvent = new AutoResetEvent(false);
        private readonly OutboxDispatcher _outboxDispatcher;
        private readonly CancellationTokenSource _stoppingTokenSource;
        private Task _loopTask;
        private Thread _thread;

        public OutboxScheduler(OutboxDispatcher outboxDispatcher)
        {
            _outboxDispatcher = outboxDispatcher;
            _stoppingTokenSource = new CancellationTokenSource();
        }

        private async Task ThreadLoop()
        {
            Console.WriteLine("Starting scheduled dispatching loop...");

            while (!_stoppingTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("loop: going to dispatch...");
                await _outboxDispatcher.DispatchPendingMessages();

                Console.WriteLine("loop: going to sleep...");
                ResetEvent.WaitOne(TimeSpan.FromMinutes(1));
                
                Console.WriteLine("loop: woke up from sleep!");
            }

            Console.WriteLine("Completed scheduled dispatching loop!");
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            
                _thread = new Thread(async () => await ThreadLoop());
                _thread.IsBackground = true;
                _thread.Start();
                
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(Stop, cancellationToken);
        }

        private void Stop()
        {
            _stoppingTokenSource?.Cancel();
            ResetEvent?.Set();

            _thread.Join(TimeSpan.FromMinutes(2));
        }

        public void Dispose()
        {
            if (!_stoppingTokenSource.IsCancellationRequested)
            {
                Stop();
            }
            
            ResetEvent?.Dispose();
            _stoppingTokenSource?.Dispose();
        }

        public void ScheduleNow()
        {
            ResetEvent.Set();
        }
    }
}