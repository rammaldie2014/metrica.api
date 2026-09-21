using Metrica.Application.Dtos;

namespace Metrica.Application.Interfaces.Excel
{
    public interface IProductExcelReader
    {
        IEnumerable<ProductExcelRowDto> ReadRows(
            Stream content,
            CancellationToken cancellationToken = default);
    }
}
