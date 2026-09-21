using Metrica.Application.Interfaces.Repositories;
using Metrica.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Metrica.Infrastructure.Persistence.Repositories
{
    public class FileLoadErrorRepository : IFileLoadErrorRepository
    {
        private readonly MetricaDbContext _context;

        public FileLoadErrorRepository(MetricaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            FileLoadError fileLoadError,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<FileLoadError>().AddAsync(
                fileLoadError,
                cancellationToken);
        }

        public async Task<(IReadOnlyList<FileLoadError> Items, int TotalCount)> GetPagedByFileLoadIdAsync(
            long fileLoadId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (fileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fileLoadId));
            }

            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            }

            if (pageSize is < 1 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            var query = _context.FileLoadErrors
                .AsNoTracking()
                .Where(error => error.FileLoadId == fileLoadId);

            var totalCount = await query.CountAsync(cancellationToken);

            var offset = ((long)pageNumber - 1) * pageSize;

            if (offset >= totalCount)
            {
                return (Array.Empty<FileLoadError>(), totalCount);
            }

            var items = await query
                .OrderBy(error => error.CreatedAt)
                .ThenBy(error => error.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

    }
}
