using Metrica.Application.Interfaces.Repositories;
using Metrica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metrica.Infrastructure.Persistence.Repositories
{
    public class FileLoadStatusHistoryRepository
        : IFileLoadStatusHistoryRepository
    {
        private readonly MetricaDbContext _context;

        public FileLoadStatusHistoryRepository(MetricaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            FileLoadStatusHistory statusHistory,
            CancellationToken cancellationToken = default)
        {
            await _context.FileLoadStatusHistories.AddAsync(
                statusHistory,
                cancellationToken);
        }

        public async Task<IReadOnlyList<FileLoadStatusHistory>> GetByFileLoadIdAsync(
            long fileLoadId,
            CancellationToken cancellationToken = default)
        {
            if (fileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fileLoadId));
            }

            return await _context.FileLoadStatusHistories
                .AsNoTracking()
                .Where(history => history.FileLoadId == fileLoadId)
                .OrderBy(history => history.CreatedAt)
                .ThenBy(history => history.Id)
                .ToListAsync(cancellationToken);
        }

    }
}
