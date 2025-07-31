using FonTech.Producer.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace FonTech.Producer
{
    public class Producer : IMessageProducer
    {
        public async Task SendMessage<T>(T message, string routingKey, string? exchange = null)
        {
            var factory = new ConnectionFactory() { HostName = "localhost"};
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var json = JsonConvert.SerializeObject(message, Formatting.Indented, // Formatting.Indented - добавляет отступы в строку, т.е. записывает не в одну строчку, а в отформатированном виде
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore//игорирует случай, когда класс внутри себя использует сам себя
                });
            var body = Encoding.UTF8.GetBytes(json);
            await channel.BasicPublishAsync(exchange: exchange, routingKey: routingKey, body: body);
            
        }
    }
}
