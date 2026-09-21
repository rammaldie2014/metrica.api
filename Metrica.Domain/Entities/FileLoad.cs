using Metrica.Domain.Enums;

namespace Metrica.Domain.Entities
{
    public class FileLoad
    {
        public long Id { get; private set; }
        public string FileName { get; private set; } = string.Empty;
        public string UserEmail { get; private set; } = string.Empty;
        public string? FilePath { get; private set; }
        public string? Period { get; private set; }
        public FileLoadStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? FinishedAt { get; private set; }

        private FileLoad()
        {
        }

        public FileLoad(string fileName, string userEmail)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "El nombre del archivo es obligatorio.",
                    nameof(fileName));
            }

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                throw new ArgumentException(
                    "El correo del usuario es obligatorio.",
                    nameof(userEmail));
            }

            FileName = fileName.Trim();
            UserEmail = userEmail.Trim();
            Status = FileLoadStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void StartProcessing()
        {
            ChangeStatus(
                FileLoadStatus.Pending,
                FileLoadStatus.InProcess);
        }

        public void MarkAsLoaded()
        {
            ChangeStatus(
                FileLoadStatus.InProcess,
                FileLoadStatus.Loaded);
        }

        public void FinishProcessing()
        {
            ChangeStatus(
                FileLoadStatus.Loaded,
                FileLoadStatus.Finished);

            FinishedAt = DateTime.UtcNow;
        }

        public void MarkAsNotified()
        {
            ChangeStatus(
                FileLoadStatus.Finished,
                FileLoadStatus.Notified);
        }

        public void Reject()
        {
            if (Status != FileLoadStatus.Pending &&
                Status != FileLoadStatus.InProcess)
            {
                throw new InvalidOperationException(
                    $"No se puede rechazar una carga en estado {Status}.");
            }

            Status = FileLoadStatus.Rejected;
            FinishedAt = DateTime.UtcNow;
        }

        public void Fail()
        {
            if (Status != FileLoadStatus.Pending &&
                Status != FileLoadStatus.InProcess &&
                Status != FileLoadStatus.Loaded)
            {
                throw new InvalidOperationException(
                    $"No se puede marcar como fallida una carga en estado {Status}.");
            }

            Status = FileLoadStatus.Failed;
            FinishedAt = DateTime.UtcNow;
        }

        public void SetFilePath(string filePath)
        {
            if (Status != FileLoadStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Solo se puede asignar el archivo a una carga pendiente.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "La ruta del archivo es obligatoria.",
                    nameof(filePath));
            }

            FilePath = filePath.Trim();
        }

        public void AssignPeriod(string period)
        {
            if (Status != FileLoadStatus.Pending &&
                Status != FileLoadStatus.InProcess)
            {
                throw new InvalidOperationException(
                    $"No se puede asignar el periodo en estado {Status}.");
            }

            if (string.IsNullOrWhiteSpace(period))
            {
                throw new ArgumentException(
                    "El periodo es obligatorio.",
                    nameof(period));
            }

            if (Period is not null)
            {
                throw new InvalidOperationException(
                    "La carga ya tiene un periodo asignado.");
            }

            Period = period.Trim();
        }

        private void ChangeStatus(
            FileLoadStatus expectedStatus,
            FileLoadStatus newStatus)
        {
            if (Status != expectedStatus)
            {
                throw new InvalidOperationException(
                    $"No se puede cambiar el estado de {Status} a {newStatus}.");
            }

            Status = newStatus;
        }

    }
}
