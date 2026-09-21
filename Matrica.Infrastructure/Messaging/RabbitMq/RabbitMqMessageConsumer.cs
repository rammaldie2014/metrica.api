using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqMessageConsumer
        : IRabbitMqMessageConsumer
    {
        private readonly IRabbitMqConnectionProvider _connectionProvider;
        private readonly ILogger<RabbitMqMessageConsumer> _logger;

        public RabbitMqMessageConsumer(
            IRabbitMqConnectionProvider connectionProvider,
            ILogger<RabbitMqMessageConsumer> logger)
        {
            _connectionProvider = connectionProvider;
            _logger = logger;
        }

        public async Task ConsumeAsync<TMessage>(
            string exchangeName,
            string queueName,
            string routingKey,
            Func<TMessage, CancellationToken, Task> handleMessageAsync,
            CancellationToken cancellationToken = default)
            where TMessage : class
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(exchangeName);
            ArgumentException.ThrowIfNullOrWhiteSpace(queueName);
            ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);
            ArgumentNullException.ThrowIfNull(handleMessageAsync);

            cancellationToken.ThrowIfCancellationRequested();

            await using var channel = await CreateChannelAsync(
                exchangeName,
                queueName,
                routingKey,
                cancellationToken);

            var consumerFailure = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var message = JsonSerializer.Deserialize<TMessage>(
                        eventArgs.Body.Span)
                        ?? throw new JsonException(
                            "El mensaje recibido no puede ser null.");

                    _logger.LogInformation(
                        "Mensaje recibido en {QueueName}. " +
                        "DeliveryTag: {DeliveryTag}.",
                        queueName,
                        eventArgs.DeliveryTag);

                    await handleMessageAsync(
                        message,
                        cancellationToken);

                    await channel.BasicAckAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation(
                        "Mensaje atendido y ACK enviado en {QueueName}. " +
                        "DeliveryTag: {DeliveryTag}.",
                        queueName,
                        eventArgs.DeliveryTag);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    // El cierre del canal deja el mensaje sin confirmar.
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Error atendiendo un mensaje en {QueueName}. " +
                        "Se detendrá el consumidor sin confirmar " +
                        "su atención. DeliveryTag: {DeliveryTag}.",
                        queueName,
                        eventArgs.DeliveryTag);

                    consumerFailure.TrySetException(exception);
                }
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Esperando mensajes en la cola {QueueName}.",
                queueName);

            try
            {
                await consumerFailure.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Se solicitó detener el consumidor de {QueueName}.",
                    queueName);
            }
        }

        private async Task<IChannel> CreateChannelAsync(
            string exchangeName,
            string queueName,
            string routingKey,
            CancellationToken cancellationToken)
        {
            var connection = await _connectionProvider
                .GetConnectionAsync(cancellationToken);

            if (!connection.IsOpen)
            {
                throw new InvalidOperationException(
                    "La conexión con RabbitMQ no está disponible.");
            }

            var channel = await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

            try
            {
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

                await channel.BasicQosAsync(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false,
                    cancellationToken: cancellationToken);

                return channel;
            }
            catch
            {
                await channel.DisposeAsync();
                throw;
            }
        }
    }
}