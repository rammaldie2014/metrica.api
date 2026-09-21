using Metrica.Application.Interfaces.Repositories;

namespace Metrica.Application.Queries.FileLoads.GetFileLoadProducts
{
    public sealed class GetFileLoadProductsQueryHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IProcessedProductRepository _processedProductRepository;

        public GetFileLoadProductsQueryHandler(
            IFileLoadRepository fileLoadRepository,
            IProcessedProductRepository processedProductRepository)
        {
            _fileLoadRepository = fileLoadRepository;
            _processedProductRepository = processedProductRepository;
        }

        public async Task<GetFileLoadProductsResponse?> HandleAsync(
            GetFileLoadProductsQuery query,
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

            var (products, totalCount) =
                await _processedProductRepository.GetPagedByFileLoadIdAsync(
                    query.FileLoadId,
                    query.PageNumber,
                    query.PageSize,
                    cancellationToken);

            var items = products
                .Select(product => new GetFileLoadProductsItemResponse(
                    product.Id,
                    product.FileLoadId,
                    product.Period,
                    product.ProductCode,
                    product.ProductName,
                    product.Description,
                    product.Price,
                    product.Stock,
                    product.CreatedAt))
                .ToList();

            return new GetFileLoadProductsResponse(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize);
        }
    }
}