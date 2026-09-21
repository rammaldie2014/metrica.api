using Metrica.Application.Interfaces.Messaging;

namespace Metrica.Worker
{
    public sealed class Worker : BackgroundService
    {
        private readonly IFileProcessingConsumer _consumer;

        public Worker(IFileProcessingConsumer consumer)
        {
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.ConsumeAsync(stoppingToken);
        }
    }
}
