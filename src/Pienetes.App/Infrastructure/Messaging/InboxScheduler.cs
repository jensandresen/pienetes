using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class InboxScheduler : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public InboxScheduler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var messagingGateway = _serviceProvider.GetRequiredService<MessagingGateway>();
                messagingGateway.RegisterReceiveCallback(OnMessage);
            }, cancellationToken);
        }

        private async Task OnMessage(string messageid, string messageType, string rawmessagebody)
        {
            var inboxDispatcher = _serviceProvider.GetRequiredService<InboxDispatcher>();
            await inboxDispatcher.Dispatch(new InboxMessage
            {
                MessageId = messageid,
                MessageType = messageType,
                Payload = rawmessagebody
            });
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}