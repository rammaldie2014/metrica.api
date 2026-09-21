using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Messaging.Contracts;

namespace Metrica.Infrastructure.Persistence.Outbox
{
    public sealed class FileProcessingOutbox : IFileProcessingOutbox
    {
        private readonly MetricaDbContext _context;

        public FileProcessingOutbox(MetricaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            ProcessFileRequested message,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var outboxMessage = new FileProcessingOutboxMessage(
                message.FileLoadId);

            await _context.FileProcessingOutboxMessages.AddAsync(
                outboxMessage,
                cancellationToken);
        }
    }
}