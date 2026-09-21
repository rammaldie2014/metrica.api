using Metrica.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Interfaces.Repositories
{
    public interface IFileLoadErrorRepository
    {
        Task AddAsync(
            FileLoadError fileLoadError,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<FileLoadError> Items, int TotalCount)> GetPagedByFileLoadIdAsync(
            long fileLoadId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

    }
}
