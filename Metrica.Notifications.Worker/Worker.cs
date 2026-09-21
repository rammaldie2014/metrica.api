using Metrica.Application.Interfaces.Messaging;

namespace Metrica.Notifications.Worker
{
    public sealed class Worker : BackgroundService
    {
        private readonly IFileLoadNotificationConsumer _consumer;

        public Worker(IFileLoadNotificationConsumer consumer)
        {
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            await _consumer.ConsumeAsync(stoppingToken);
        }
    }
}
