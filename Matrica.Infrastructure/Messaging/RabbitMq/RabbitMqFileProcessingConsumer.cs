using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqFileProcessingConsumer
        : IFileProcessingConsumer
    {
        private readonly IRabbitMqMessageConsumer _consumer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _rabbitMqOptions;
        private readonly FileProcessingQueueOptions _queueOptions;
        private readonly ILogger<RabbitMqFileProcessingConsumer> _logger;

        public RabbitMqFileProcessingConsumer(
            IRabbitMqMessageConsumer consumer,
            IOptions<RabbitMqOptions> rabbitMqOptions,
            IOptions<FileProcessingQueueOptions> queueOptions,
            ILogger<RabbitMqFileProcessingConsumer> logger,
            IServiceScopeFactory scopeFactory)
        {
            _consumer = consumer;
            _rabbitMqOptions = rabbitMqOptions.Value;
            _queueOptions = queueOptions.Value;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task ConsumeAsync(
            CancellationToken cancellationToken = default)
        {
            return _consumer.ConsumeAsync<ProcessFileRequested>(
                _rabbitMqOptions.ExchangeName,
                _queueOptions.QueueName,
                _queueOptions.RoutingKey,
                ProcessWithRetriesAsync,
                cancellationToken);
        }

        private async Task RecordFailureAsync(
            long fileLoadId,
            string errorCode,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<IFileLoadFailureHandler>();

            await handler.HandleAsync(
                fileLoadId,
                errorCode,
                errorMessage,
                cancellationToken);
        }

        private async Task ProcessWithRetriesAsync(
            ProcessFileRequested message,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1;
                 attempt <= _queueOptions.MaxProcessingAttempts;
                 attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();

                    var handler = scope.ServiceProvider
                        .GetRequiredService<IFileProcessingHandler>();

                    await handler.HandleAsync(message, cancellationToken);

                    return;
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    if (attempt == _queueOptions.MaxProcessingAttempts)
                    {
                        _logger.LogError(
                            exception,
                            "Se agotaron los {MaxAttempts} intentos para " +
                            "la carga {FileLoadId}.",
                            _queueOptions.MaxProcessingAttempts,
                            message.FileLoadId);

                        await RecordFailureAsync(
                            message.FileLoadId,
                            "PROCESSING_ATTEMPTS_EXHAUSTED",
                            $"El procesamiento falló después de {attempt} intentos. " +
                            exception.Message,
                            cancellationToken);

                        return;
                    }

                    _logger.LogWarning(
                        exception,
                        "Falló el intento {Attempt} de {MaxAttempts} para " +
                        "la carga {FileLoadId}. Se reintentará en {DelaySeconds} segundos.",
                        attempt,
                        _queueOptions.MaxProcessingAttempts,
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
