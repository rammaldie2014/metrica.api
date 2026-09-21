
namespace Metrica.Domain.Entities
{
    public class FileLoadError
    {
        public long Id { get; private set; }
        public long FileLoadId { get; private set; }
        public int? RowNumber { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        private FileLoadError()
        {
        }

        public FileLoadError(
            long fileLoadId,
            string code,
            string message,
            int? rowNumber = null)
        {
            if (fileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fileLoadId),
                    "El identificador de la carga debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException(
                    "El código de la incidencia es obligatorio.",
                    nameof(code));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "El mensaje de la incidencia es obligatorio.",
                    nameof(message));
            }

            if (rowNumber.HasValue && rowNumber.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rowNumber),
                    "El número de fila debe ser mayor que cero.");
            }

            FileLoadId = fileLoadId;
            Code = code.Trim();
            Message = message.Trim();
            RowNumber = rowNumber;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
