using Metrica.Application.Interfaces.Repositories;

namespace Metrica.Application.Queries.FileLoads.GetFileLoadErrors
{
    public sealed class GetFileLoadErrorsQueryHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IFileLoadErrorRepository _fileLoadErrorRepository;

        public GetFileLoadErrorsQueryHandler(
            IFileLoadRepository fileLoadRepository,
            IFileLoadErrorRepository fileLoadErrorRepository)
        {
            _fileLoadRepository = fileLoadRepository;
            _fileLoadErrorRepository = fileLoadErrorRepository;
        }

        public async Task<GetFileLoadErrorsResponse?> HandleAsync(
            GetFileLoadErrorsQuery query,
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

            var (errors, totalCount) =
                await _fileLoadErrorRepository.GetPagedByFileLoadIdAsync(
                    query.FileLoadId,
                    query.PageNumber,
                    query.PageSize,
                    cancellationToken);

            var items = errors
                .Select(error => new GetFileLoadErrorsItemResponse(
                    error.Id,
                    error.FileLoadId,
                    error.RowNumber,
                    error.Code,
                    error.Message,
                    error.CreatedAt))
                .ToList();

            return new GetFileLoadErrorsResponse(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize);
        }
    }
}