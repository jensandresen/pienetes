using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pienetes.App.Infrastructure.Messaging
{
    public class MessagingGateway : IDisposable
    {
        private const string QueueName = "pienetes-inbox";
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private MessagingGateway(ConnectionFactory connectionFactory, IConnection connection, IModel channel)
        {
            _connectionFactory = connectionFactory;
            _connection = connection;
            _channel = channel;
        }

        private void Stop()
        {
            _channel.Close();
            _connection.Close();
        }
        
        public void Dispose()
        {
            Stop();
            
            _channel.Dispose();
            _connection.Dispose();
        }
        
        public static MessagingGateway Init(params string[] topics)
        {
            var connectionFactory = new ConnectionFactory();
            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            foreach (var topic in topics)
            {
                channel.ExchangeDeclare(topic, ExchangeType.Fanout);
                channel.QueueBind(
                    queue: QueueName,
                    exchange: topic,
                    routingKey: ""
                );
            }

            return new MessagingGateway(connectionFactory, connection, channel);
        }

        public Task Publish(string topic, string messageId, string messageType, string payload)
        {
            var properties = _channel.CreateBasicProperties();
            properties.MessageId = messageId;
            properties.Type = messageType;
            properties.ContentType = "application/json";
                
            _channel.BasicPublish(
                exchange: topic,
                routingKey: "",
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(payload)
            );
            
            return Task.CompletedTask;  
        }
        
        public void RegisterReceiveCallback(TransportMessageHandler callback)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (sender, args) =>
            {
                var messageId = args.BasicProperties.MessageId;
                var messageType = args.BasicProperties.Type;
                var body = args.Body.ToArray();
                var rawMessageBody = Encoding.UTF8.GetString(body);

                try
                {
                    await callback(messageId, messageType, rawMessageBody);
                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                    throw;
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }

        public delegate Task TransportMessageHandler(string messageId, string messageType, string rawMessageBody);
    }
}