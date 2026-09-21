using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Messaging.Contracts;

namespace Metrica.Infrastructure.Persistence.Outbox
{
    public sealed class FileLoadNotificationOutbox
        : IFileLoadNotificationOutbox
    {
        private readonly MetricaDbContext _context;

        public FileLoadNotificationOutbox(MetricaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            FileLoadNotificationRequested message,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var outboxMessage = new FileLoadNotificationOutboxMessage(
                message.FileLoadId);

            await _context.FileLoadNotificationOutboxMessages.AddAsync(
                outboxMessage,
                cancellationToken);
        }
    }
}