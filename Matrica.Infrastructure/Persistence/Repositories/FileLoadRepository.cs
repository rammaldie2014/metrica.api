using Metrica.Application.Interfaces.Repositories;
using Metrica.Domain.Entities;
using Metrica.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace Metrica.Infrastructure.Persistence.Repositories
{
    public class FileLoadRepository : IFileLoadRepository
    {
        private readonly MetricaDbContext _context;

        public FileLoadRepository(MetricaDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            FileLoad fileLoad,
            CancellationToken cancellationToken = default)
        {
            await _context.FileLoads.AddAsync(
                fileLoad,
                cancellationToken);
        }

        public async Task<FileLoad?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
        {
            return await _context.FileLoads.FirstOrDefaultAsync(
                fileLoad => fileLoad.Id == id,
                cancellationToken);
        }

        public async Task<bool> TryReservePeriodAsync(
            string period,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(period);

            period = period.Trim();

            var blockingLoadExists = await _context.FileLoads
                .FromSqlInterpolated($"""
            SELECT *
            FROM [FileLoads] WITH (UPDLOCK, HOLDLOCK)
            WHERE [Period] = {period}
              AND [Status] IN (
                  {FileLoadStatus.Pending.ToString()},
                  {FileLoadStatus.InProcess.ToString()},
                  {FileLoadStatus.Loaded.ToString()},
                  {FileLoadStatus.Finished.ToString()},
                  {FileLoadStatus.Notified.ToString()}
              )
            """)
                .AsNoTracking()
                .AnyAsync(cancellationToken);

            return !blockingLoadExists;
        }

        public async Task<IReadOnlyList<long>> GetPendingNotificationIdsAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(batchSize),
                    "El tamaño del lote debe ser mayor que cero.");
            }

            return await _context.FileLoads
                .Where(fileLoad => fileLoad.Status == FileLoadStatus.Finished)
                .OrderBy(fileLoad => fileLoad.Id)
                .Select(fileLoad => fileLoad.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<FileLoad> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            }

            if (pageSize is < 1 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            var query = _context.FileLoads.AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var offset = ((long)pageNumber - 1) * pageSize;

            if (offset >= totalCount)
            {
                return (Array.Empty<FileLoad>(), totalCount);
            }

            var items = await _context.FileLoads
                .FromSqlInterpolated(
                    $"EXEC dbo.sp_GetFileLoads @PageNumber={pageNumber}, @PageSize={pageSize}")
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }


    }
}
