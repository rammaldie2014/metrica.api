namespace Metrica.Infrastructure.Persistence.Outbox
{
    public sealed class FileProcessingOutboxMessage
    {
        public long Id { get; private set; }

        public long FileLoadId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? PublishedAt { get; private set; }

        private FileProcessingOutboxMessage()
        {
        }

        public FileProcessingOutboxMessage(long fileLoadId)
        {
            if (fileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fileLoadId),
                    "El identificador de la carga debe ser mayor que cero.");
            }

            FileLoadId = fileLoadId;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsPublished()
        {
            PublishedAt ??= DateTime.UtcNow;
        }
    }
}