using Metrica.Domain.Enums;

namespace Metrica.Application.Queries.FileLoads.GetFileLoads
{
    public sealed record GetFileLoadsItemResponse(
        long Id,
        string FileName,
        string UserEmail,
        string? Period,
        FileLoadStatus Status,
        DateTime CreatedAt,
        DateTime? FinishedAt);
}