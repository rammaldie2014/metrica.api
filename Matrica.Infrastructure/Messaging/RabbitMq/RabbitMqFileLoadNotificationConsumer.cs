using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Notifications;
using Metrica.Application.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqFileLoadNotificationConsumer
        : IFileLoadNotificationConsumer
    {
        private readonly IRabbitMqMessageConsumer _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _rabbitMqOptions;
        private readonly FileNotificationQueueOptions _queueOptions;
        private readonly ILogger<RabbitMqFileLoadNotificationConsumer> _logger;

        public RabbitMqFileLoadNotificationConsumer(
            IRabbitMqMessageConsumer consumer,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IOptions<FileNotificationQueueOptions> queueOptions,
            ILogger<RabbitMqFileLoadNotificationConsumer> logger)
        {
            _consumer = consumer;
            _scopeFactory = scopeFactory;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        public Task ConsumeAsync(
            CancellationToken cancellationToken = default)
        {
            return _consumer.ConsumeAsync<FileLoadNotificationRequested>(
                _rabbitMqOptions.ExchangeName,
                _queueOptions.QueueName,
                _queueOptions.RoutingKey,
                NotifyWithRetriesAsync,
                cancellationToken);
        }

        private async Task NotifyWithRetriesAsync(
            FileLoadNotificationRequested message,
            CancellationToken cancellationToken)
        {
            if (message.FileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(message.FileLoadId),
                    "El identificador de la carga debe ser mayor que cero.");
            }

            for (var attempt = 1; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await using var scope =
                        _scopeFactory.CreateAsyncScope();

                    var handler = scope.ServiceProvider
                        .GetRequiredService<IFileLoadNotificationHandler>();

                    await handler.HandleAsync(
                        message.FileLoadId,
                        cancellationToken);

                    return;
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    if (attempt >= _queueOptions.MaxNotificationAttempts)
                    {
                        _logger.LogError(
                            exception,
                            "Se agotaron los {MaxAttempts} intentos de " +
                            "notificación para la carga {FileLoadId}.",
                            _queueOptions.MaxNotificationAttempts,
                            message.FileLoadId);

                        throw;
                    }

                    _logger.LogWarning(
                        exception,
                        "Falló el intento {Attempt} de {MaxAttempts} " +
                        "de notificación para la carga {FileLoadId}. " +
                        "Se reintentará en {DelaySeconds} segundos.",
                        attempt,
                        _queueOptions.MaxNotificationAttempts,
                        message.FileLoadId,
                        _queueOptions.RetryDelaySeconds);
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(_queueOptions.RetryDelaySeconds),
                    cancellationToken);
            }
        }
    }
}