using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class OutboxScheduler : IHostedService, IDisposable, IOutboxScheduler
    {
        private static readonly AutoResetEvent ResetEvent = new AutoResetEvent(false);
        private readonly OutboxDispatcher _outboxDispatcher;
        private readonly CancellationTokenSource _stoppingTokenSource;
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