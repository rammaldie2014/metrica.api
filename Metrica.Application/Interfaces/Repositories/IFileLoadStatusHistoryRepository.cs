using Metrica.Domain.Entities;

namespace Metrica.Application.Interfaces.Repositories
{
    public interface IFileLoadStatusHistoryRepository
    {
        Task AddAsync(
            FileLoadStatusHistory statusHistory,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<FileLoadStatusHistory>> GetByFileLoadIdAsync(
            long fileLoadId,
            CancellationToken cancellationToken = default);

    }
}
