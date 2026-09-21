using Metrica.Domain.Enums;


namespace Metrica.Domain.Entities
{
    public class FileLoadStatusHistory
    {
        public long Id { get; private set; }
        public long FileLoadId { get; private set; }
        public FileLoadStatus PreviousStatus { get; private set; }
        public FileLoadStatus NewStatus { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private FileLoadStatusHistory()
        {
        }

        public FileLoadStatusHistory(
            long fileLoadId,
            FileLoadStatus previousStatus,
            FileLoadStatus newStatus,
            string? description = null)
        {
            if (fileLoadId <= 0)
                throw new ArgumentException(
                    "El identificador de la carga debe ser mayor que cero.",
                    nameof(fileLoadId));

            if (previousStatus == newStatus)
                throw new ArgumentException(
                    "El nuevo estado debe ser diferente al anterior.",
                    nameof(newStatus));

            FileLoadId = fileLoadId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            Description = description?.Trim();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
