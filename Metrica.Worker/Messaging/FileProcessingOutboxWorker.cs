using Metrica.Application.Interfaces.Messaging;
using Microsoft.Extensions.Options;

namespace Metrica.Worker.Messaging
{
    public sealed class FileProcessingOutboxWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FileProcessingOutboxOptions _options;
        private readonly ILogger<FileProcessingOutboxWorker> _logger;

        public FileProcessingOutboxWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<FileProcessingOutboxOptions> options,
            ILogger<FileProcessingOutboxWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await using var scope =
                            _scopeFactory.CreateAsyncScope();

                        var dispatcher = scope.ServiceProvider
                            .GetRequiredService<IFileProcessingOutboxDispatcher>();

                        await dispatcher.DispatchPendingAsync(
                            _options.BatchSize,
                            stoppingToken);
                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(
                            exception,
                            "Error despachando el Outbox de procesamiento. " +
                            "Se reintentará en el próximo ciclo.");
                    }

                    await Task.Delay(
                        TimeSpan.FromSeconds(
                            _options.PollingIntervalSeconds),
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Se detuvo el despacho del Outbox de procesamiento.");
            }
        }
    }
}