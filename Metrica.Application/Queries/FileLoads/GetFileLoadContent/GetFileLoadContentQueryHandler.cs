using Metrica.Application.Interfaces.Excel;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Application.Interfaces.Storage;

namespace Metrica.Application.Queries.FileLoads.GetFileLoadContent
{
    public sealed class GetFileLoadContentQueryHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IProductExcelReader _productExcelReader;

        public GetFileLoadContentQueryHandler(
            IFileLoadRepository fileLoadRepository,
            IFileStorage fileStorage,
            IProductExcelReader productExcelReader)
        {
            _fileLoadRepository = fileLoadRepository;
            _fileStorage = fileStorage;
            _productExcelReader = productExcelReader;
        }

        public async Task<GetFileLoadContentResponse?> HandleAsync(
            GetFileLoadContentQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (query.FileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(query.FileLoadId),
                    "El identificador de la carga debe ser mayor que cero.");
            }

            if (query.PageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(query.PageNumber),
                    "El número de página debe ser mayor que cero.");
            }

            if (query.PageSize is < 1 or > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(query.PageSize),
                    "El tamaño de página debe estar entre 1 y 100.");
            }

            var fileLoad = await _fileLoadRepository.GetByIdAsync(
                query.FileLoadId,
                cancellationToken);

            if (fileLoad is null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(fileLoad.FilePath))
            {
                throw new InvalidOperationException(
                    $"La carga {fileLoad.Id} no tiene un archivo asignado.");
            }

            await using var content = await _fileStorage.OpenReadAsync(
                fileLoad.FilePath,
                cancellationToken);

            var offset = ((long)query.PageNumber - 1) * query.PageSize;
            var items = new List<GetFileLoadContentItemResponse>();
            var totalCount = 0;

            foreach (var row in _productExcelReader.ReadRows(
                content,
                cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (totalCount >= offset && items.Count < query.PageSize)
                {
                    items.Add(new GetFileLoadContentItemResponse(
                        row.RowNumber,
                        row.Period,
                        row.ProductCode,
                        row.ProductName,
                        row.Description,
                        row.Price,
                        row.Stock));
                }

                totalCount = checked(totalCount + 1);
            }

            return new GetFileLoadContentResponse(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize);
        }
    }
}