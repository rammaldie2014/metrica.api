namespace Metrica.Application.Queries.FileLoads.GetFileLoadContent
{
    public sealed record GetFileLoadContentResponse(
        IReadOnlyList<GetFileLoadContentItemResponse> Items,
        int TotalCount,
        int PageNumber,
        int PageSize);
}