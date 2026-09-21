using Metrica.Application.Interfaces.Repositories;
using Metrica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Persistence.Repositories
{
    public class ProcessedProductRepository : IProcessedProductRepository
    {
        private readonly MetricaDbContext _context;

        public ProcessedProductRepository(MetricaDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(
            IEnumerable<ProcessedProduct> products,
            CancellationToken cancellationToken = default)
        {
            await _context.ProcessedProducts.AddRangeAsync(
                products,
                cancellationToken);
        }

        public async Task<IReadOnlyCollection<string>> GetExistingProductCodesAsync(
            IEnumerable<string> productCodes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(productCodes);
            cancellationToken.ThrowIfCancellationRequested();

            var existingCodes = new List<string>();

            var batches = productCodes
                .Distinct()
                .Chunk(1000);

            foreach (var batch in batches)
            {
                var matchingCodes = await _context.ProcessedProducts
                    .Where(product => batch.Contains(product.ProductCode))
                    .Select(product => product.ProductCode)
                    .ToListAsync(cancellationToken);

                existingCodes.AddRange(matchingCodes);
            }

            return existingCodes;
        }

        public async Task<(IReadOnlyList<ProcessedProduct> Items, int TotalCount)> GetPagedByFileLoadIdAsync(
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

            var query = _context.ProcessedProducts
                .AsNoTracking()
                .Where(product => product.FileLoadId == fileLoadId);

            var totalCount = await query.CountAsync(cancellationToken);

            var offset = ((long)pageNumber - 1) * pageSize;

            if (offset >= totalCount)
            {
                return (Array.Empty<ProcessedProduct>(), totalCount);
            }

            var items = await query
                .OrderBy(product => product.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

    }
}
