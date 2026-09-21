namespace Metrica.Application.Queries.FileLoads.GetFileLoads
{
    public sealed record GetFileLoadsResponse(
        IReadOnlyList<GetFileLoadsItemResponse> Items,
        int TotalCount,
        int PageNumber,
        int PageSize);
}