using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrica.Infrastructure.Persistence.Outbox
{
    public sealed class FileProcessingOutboxDispatcher
        : IFileProcessingOutboxDispatcher
    {
        private readonly MetricaDbContext _context;
        private readonly IFileProcessingPublisher _publisher;
        private readonly ILogger<FileProcessingOutboxDispatcher> _logger;

        public FileProcessingOutboxDispatcher(
            MetricaDbContext context,
            IFileProcessingPublisher publisher,
            ILogger<FileProcessingOutboxDispatcher> logger)
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
                .FileProcessingOutboxMessages
                .Where(message => message.PublishedAt == null)
                .OrderBy(message => message.CreatedAt)
                .ThenBy(message => message.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            foreach (var pendingMessage in pendingMessages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _publisher.PublishAsync(
                    new ProcessFileRequested(pendingMessage.FileLoadId),
                    cancellationToken);

                pendingMessage.MarkAsPublished();

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Solicitud de procesamiento publicada. " +
                    "OutboxId: {OutboxId}. FileLoadId: {FileLoadId}.",
                    pendingMessage.Id,
                    pendingMessage.FileLoadId);
            }
        }
    }
}