using Metrica.Application.Interfaces.Repositories;

namespace Metrica.Application.Queries.FileLoads.GetFileLoadStatusHistory
{
    public sealed class GetFileLoadStatusHistoryQueryHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IFileLoadStatusHistoryRepository _statusHistoryRepository;

        public GetFileLoadStatusHistoryQueryHandler(
            IFileLoadRepository fileLoadRepository,
            IFileLoadStatusHistoryRepository statusHistoryRepository)
        {
            _fileLoadRepository = fileLoadRepository;
            _statusHistoryRepository = statusHistoryRepository;
        }

        public async Task<IReadOnlyList<GetFileLoadStatusHistoryResponse>?> HandleAsync(
            GetFileLoadStatusHistoryQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (query.FileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(query.FileLoadId),
                    "El identificador de la carga debe ser mayor que cero.");
            }

            var fileLoad = await _fileLoadRepository.GetByIdAsync(
                query.FileLoadId,
                cancellationToken);

            if (fileLoad is null)
            {
                return null;
            }

            var history = await _statusHistoryRepository.GetByFileLoadIdAsync(
                query.FileLoadId,
                cancellationToken);

            return history
                .Select(item => new GetFileLoadStatusHistoryResponse(
                    item.Id,
                    item.FileLoadId,
                    item.PreviousStatus,
                    item.NewStatus,
                    item.Description,
                    item.CreatedAt))
                .ToList();
        }
    }
}