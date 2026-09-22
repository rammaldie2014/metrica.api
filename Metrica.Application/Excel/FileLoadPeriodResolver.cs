using Metrica.Application.Interfaces.Excel;

namespace Metrica.Application.Excel
{
    public sealed class FileLoadPeriodResolver : IFileLoadPeriodResolver
    {
        private readonly IProductExcelReader _productExcelReader;

        public FileLoadPeriodResolver(IProductExcelReader productExcelReader)
        {
            _productExcelReader = productExcelReader;
        }

        public string Resolve(
            Stream content,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            cancellationToken.ThrowIfCancellationRequested();

            if (!content.CanRead || !content.CanSeek)
            {
                throw new ArgumentException(
                    "El archivo debe permitir lectura y reposicionamiento.",
                    nameof(content));
            }

            var originalPosition = content.Position;

            try
            {
                content.Position = 0;

                string? period = null;

                foreach (var row in _productExcelReader.ReadRows(
                    content,
                    cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var rowPeriod = row.Period?.Trim();

                    if (string.IsNullOrWhiteSpace(rowPeriod))
                    {
                        throw new InvalidDataException(
                            $"La fila {row.RowNumber} no tiene periodo.");
                    }

                    if (rowPeriod.Length > 20)
                    {
                        throw new InvalidDataException(
                            $"La fila {row.RowNumber} tiene un periodo " +
                            "que supera los 20 caracteres.");
                    }

                    if (period is null)
                    {
                        period = rowPeriod;
                        continue;
                    }

                    if (!string.Equals(
                        period,
                        rowPeriod,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException(
                            $"La fila {row.RowNumber} tiene el periodo " +
                            $"'{rowPeriod}', pero la carga corresponde " +
                            $"al periodo '{period}'.");
                    }
                }

                return period ?? throw new InvalidDataException(
                    "El Excel no contiene filas con datos para procesar.");
            }
            finally
            {
                content.Position = originalPosition;
            }
        }
    }
}