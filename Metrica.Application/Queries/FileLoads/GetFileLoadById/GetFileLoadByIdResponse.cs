using Metrica.Domain.Enums;

namespace Metrica.Application.Queries.FileLoads.GetFileLoadById
{
    public sealed record GetFileLoadByIdResponse(
        long Id,
        string FileName,
        string UserEmail,
        string? Period,
        FileLoadStatus Status,
        DateTime CreatedAt,
        DateTime? FinishedAt);
}
