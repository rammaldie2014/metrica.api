using Metrica.Domain.Enums;

namespace Metrica.Application.Queries.FileLoads.GetFileLoadStatusHistory
{
    public sealed record GetFileLoadStatusHistoryResponse(
        long Id,
        long FileLoadId,
        FileLoadStatus PreviousStatus,
        FileLoadStatus NewStatus,
        string? Description,
        DateTime CreatedAt);
}