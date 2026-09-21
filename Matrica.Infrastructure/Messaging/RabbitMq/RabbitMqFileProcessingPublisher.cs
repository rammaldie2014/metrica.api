using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Messaging.Contracts;
using Microsoft.Extensions.Options;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqFileProcessingPublisher
        : IFileProcessingPublisher
    {
        private readonly RabbitMqMessagePublisher _publisher;
        private readonly RabbitMqOptions _rabbitMqOptions;
        private readonly FileProcessingQueueOptions _queueOptions;

        public RabbitMqFileProcessingPublisher(
            RabbitMqMessagePublisher publisher,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IOptions<FileProcessingQueueOptions> queueOptions)
        {
            _publisher = publisher;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _queueOptions = queueOptions.Value;
        }

        public Task PublishAsync(
            ProcessFileRequested message,
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