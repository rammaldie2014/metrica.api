using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public interface IRabbitMqMessageConsumer
    {
        Task ConsumeAsync<TMessage>(
            string exchangeName,
            string queueName,
            string routingKey,
            Func<TMessage, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default)
            where TMessage : class;
    }
}
