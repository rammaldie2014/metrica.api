using ExcelDataReader;
using Metrica.Application.Dtos;
using Metrica.Application.Interfaces.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Excel
{
    public sealed class ProductExcelReader : IProductExcelReader
    {
        public IEnumerable<ProductExcelRowDto> ReadRows(
            Stream content,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            cancellationToken.ThrowIfCancellationRequested();

            using var reader = ExcelReaderFactory.CreateOpenXmlReader(
                content,
                new ExcelReaderConfiguration
                {
                    LeaveOpen = true
                });

            if (!reader.Read())
            {
                throw new InvalidDataException(
                    "El Excel no contiene una fila de encabezados.");
            }

            var columns = ReadHeaders(reader);

            ValidateRequiredColumn(columns, "Periodo");
            ValidateRequiredColumn(columns, "CodigoProducto");

            var rowNumber = 1;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!reader.Read())
                {
                    break;
                }

                rowNumber++;

                if (IsEmptyRow(reader))
                {
                    continue;
                }

                yield return new ProductExcelRowDto
                {
                    RowNumber = rowNumber,
                    Period = ReadCell(reader, columns, "Periodo"),
                    ProductCode = ReadCell(reader, columns, "CodigoProducto"),
                    ProductName = ReadCell(reader, columns, "NombreProducto"),
                    Description = ReadCell(reader, columns, "Descripcion"),
                    Price = ReadCell(reader, columns, "Precio"),
                    Stock = ReadCell(reader, columns, "Stock")
                };
            }
        }

        private static Dictionary<string, int> ReadHeaders(
            IExcelDataReader reader)
        {
            var columns = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < reader.FieldCount; index++)
            {
                var header = GetCellText(reader, index);

                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                if (!columns.TryAdd(header, index))
                {
                    throw new InvalidDataException(
                        $"El encabezado '{header}' está duplicado.");
                }
            }

            return columns;
        }

        private static void ValidateRequiredColumn(
            IReadOnlyDictionary<string, int> columns,
            string columnName)
        {
            if (!columns.ContainsKey(columnName))
            {
                throw new InvalidDataException(
                    $"Falta la columna obligatoria '{columnName}'.");
            }
        }

        private static bool IsEmptyRow(IExcelDataReader reader)
        {
            for (var index = 0; index < reader.FieldCount; index++)
            {
                if (!string.IsNullOrWhiteSpace(GetCellText(reader, index)))
                {
                    return false;
                }
            }

            return true;
        }

        private static string ReadCell(
            IExcelDataReader reader,
            IReadOnlyDictionary<string, int> columns,
            string columnName)
        {
            return columns.TryGetValue(columnName, out var index)
                ? GetCellText(reader, index)
                : string.Empty;
        }

        private static string GetCellText(
            IExcelDataReader reader,
            int index)
        {
            return Convert.ToString(
                reader.GetValue(index),
                CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
        }
    }
}
