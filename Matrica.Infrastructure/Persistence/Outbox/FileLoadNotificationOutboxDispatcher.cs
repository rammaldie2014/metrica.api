using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrica.Infrastructure.Persistence.Outbox
{
    public sealed class FileLoadNotificationOutboxDispatcher
        : IFileLoadNotificationOutboxDispatcher
    {
        private readonly MetricaDbContext _context;
        private readonly IFileLoadNotificationPublisher _publisher;
        private readonly ILogger<FileLoadNotificationOutboxDispatcher> _logger;

        public FileLoadNotificationOutboxDispatcher(
            MetricaDbContext context,
            IFileLoadNotificationPublisher publisher,
            ILogger<FileLoadNotificationOutboxDispatcher> logger)
        {
            _context = context;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task DispatchPendingAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(batchSize),
                    "El tamaño del lote debe ser mayor que cero.");
            }

            var pendingMessages = await _context
                .FileLoadNotificationOutboxMessages
                .Where(message => message.PublishedAt == null)
                .OrderBy(message => message.CreatedAt)
                .ThenBy(message => message.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            foreach (var pendingMessage in pendingMessages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _publisher.PublishAsync(
                    new FileLoadNotificationRequested(
                        pendingMessage.FileLoadId),
                    cancellationToken);

                pendingMessage.MarkAsPublished();

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Solicitud de notificación publicada. " +
                    "OutboxId: {OutboxId}. FileLoadId: {FileLoadId}.",
                    pendingMessage.Id,
                    pendingMessage.FileLoadId);
            }
        }
    }
}