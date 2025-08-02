using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using FonTech.Domain.Settings;
using RabbitMQ.Client.Events;
using System.Text;
using System.Diagnostics;

namespace FonTech.Consumer
{
    public class RabbitMqListener : BackgroundService
    {
        private  readonly IConnection _connection;
        private  readonly IChannel _channel;
        private readonly IOptions<RabbitMqSettings> _options;

        public RabbitMqListener(IOptions<RabbitMqSettings> options)
        {
            _options = options;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();//not good
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.QueueDeclareAsync(_options.Value.QueueName, durable: true, exclusive: false, autoDelete: false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (obj, basicDeliver) =>
            {
                var content = Encoding.UTF8.GetString(basicDeliver.Body.ToArray());
                Debug.WriteLine($"Получено сообщение: {content}");

                await _channel.BasicAckAsync(basicDeliver.DeliveryTag, multiple: false);

            };
            await _channel.BasicConsumeAsync(queue: _options.Value.QueueName, autoAck: false, consumer: consumer);
        }

    }
}
