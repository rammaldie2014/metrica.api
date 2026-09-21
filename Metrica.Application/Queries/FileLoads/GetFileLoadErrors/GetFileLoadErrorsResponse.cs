namespace Metrica.Application.Queries.FileLoads.GetFileLoadErrors
{
    public sealed record GetFileLoadErrorsResponse(
        IReadOnlyList<GetFileLoadErrorsItemResponse> Items,
        int TotalCount,
        int PageNumber,
        int PageSize);
}