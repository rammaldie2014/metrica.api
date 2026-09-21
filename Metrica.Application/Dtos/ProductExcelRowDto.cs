

namespace Metrica.Application.Dtos
{
    public sealed class ProductExcelRowDto
    {
        public int RowNumber { get; init; }

        public string Period { get; init; } = string.Empty;

        public string ProductCode { get; init; } = string.Empty;

        public string ProductName { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string Price { get; init; } = string.Empty;

        public string Stock { get; init; } = string.Empty;
    }
}
