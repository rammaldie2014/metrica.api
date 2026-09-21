using Metrica.Domain.Entities;

namespace Metrica.Application.Interfaces.Repositories
{
    public interface IProcessedProductRepository
    {
        Task AddRangeAsync(
            IEnumerable<ProcessedProduct> products,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<string>> GetExistingProductCodesAsync(
            IEnumerable<string> productCodes,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<ProcessedProduct> Items, int TotalCount)> GetPagedByFileLoadIdAsync(
            long fileLoadId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

    }
}
