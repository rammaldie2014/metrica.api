using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqMessagePublisher : IAsyncDisposable
    {
        private readonly IRabbitMqConnectionProvider _connectionProvider;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private IChannel? _channel;
        private bool _disposed;

        public RabbitMqMessagePublisher(
            IRabbitMqConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task PublishAsync<TMessage>(
            TMessage message,
            string exchangeName,
            string queueName,
            string routingKey,
            CancellationToken cancellationToken = default)
            where TMessage : class
        {
            ArgumentNullException.ThrowIfNull(message);
            ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);
            ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
            ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                var channel = await GetChannelAsync(cancellationToken);

                await channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: cancellationToken);

                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey,
                    cancellationToken: cancellationToken);

                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    Persistent = true,
                    MessageId = Guid.NewGuid().ToString()
                };

                await channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<IChannel> GetChannelAsync(
            CancellationToken cancellationToken)
        {
            var connection = await _connectionProvider
                .GetConnectionAsync(cancellationToken);

            if (!connection.IsOpen)
            {
                throw new InvalidOperationException(
                    "La conexión con RabbitMQ no está disponible.");
            }

            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            if (_channel is not null)
            {
                await _channel.DisposeAsync();
                _channel = null;
            }

            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

            _channel = await connection.CreateChannelAsync(
                options: channelOptions,
                cancellationToken: cancellationToken);

            return _channel;
        }

        public async ValueTask DisposeAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                if (_channel is not null)
                {
                    await _channel.DisposeAsync();
                    _channel = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
