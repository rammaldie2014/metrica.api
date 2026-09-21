using Metrica.Application.Interfaces.Repositories;

namespace Metrica.Application.Queries.FileLoads.GetFileLoads
{
    public sealed class GetFileLoadsQueryHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;

        public GetFileLoadsQueryHandler(
            IFileLoadRepository fileLoadRepository)
        {
            _fileLoadRepository = fileLoadRepository;
        }

        public async Task<GetFileLoadsResponse> HandleAsync(
            GetFileLoadsQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

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

            var (fileLoads, totalCount) =
                await _fileLoadRepository.GetPagedAsync(
                    query.PageNumber,
                    query.PageSize,
                    cancellationToken);

            var items = fileLoads
                .Select(load => new GetFileLoadsItemResponse(
                    load.Id,
                    load.FileName,
                    load.UserEmail,
                    load.Period,
                    load.Status,
                    load.CreatedAt,
                    load.FinishedAt))
                .ToList();

            return new GetFileLoadsResponse(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize);
        }
    }
}