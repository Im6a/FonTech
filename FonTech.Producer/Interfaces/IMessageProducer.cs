using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FonTech.Producer.Interfaces
{
    public interface IMessageProducer
    {
        Task SendMessage<T>(T message, string routingKey, string? exchange = default);
    }
}
