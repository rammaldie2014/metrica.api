using Metrica.Domain.Entities;


namespace Metrica.Application.Interfaces.Repositories
{
    public interface IFileLoadRepository
    {
        Task AddAsync(
            FileLoad fileLoad,
            CancellationToken cancellationToken = default);

        Task<FileLoad?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default);

        Task<bool> TryReservePeriodAsync(
            string period,
            long fileLoadId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<long>> GetPendingNotificationIdsAsync(
            int batchSize,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<FileLoad> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);


    }
}
