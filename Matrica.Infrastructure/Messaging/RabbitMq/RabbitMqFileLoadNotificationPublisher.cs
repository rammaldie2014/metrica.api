using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Messaging.Contracts;
using Microsoft.Extensions.Options;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqFileLoadNotificationPublisher
        : IFileLoadNotificationPublisher
    {
        private readonly RabbitMqMessagePublisher _publisher;
        private readonly RabbitMqOptions _rabbitMqOptions;
        private readonly FileNotificationQueueOptions _queueOptions;

        public RabbitMqFileLoadNotificationPublisher(
            RabbitMqMessagePublisher publisher,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IOptions<FileNotificationQueueOptions> queueOptions)
        {
            _publisher = publisher;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _queueOptions = queueOptions.Value;
        }

        public Task PublishAsync(
            FileLoadNotificationRequested message,
            CancellationToken cancellationToken = default)
        {
            return _publisher.PublishAsync(
                message,
                _rabbitMqOptions.ExchangeName,
                _queueOptions.QueueName,
                _queueOptions.RoutingKey,
                cancellationToken);
        }
    }
}