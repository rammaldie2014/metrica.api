namespace Metrica.Application.Queries.FileLoads.GetFileLoadProducts
{
    public sealed record GetFileLoadProductsResponse(
        IReadOnlyList<GetFileLoadProductsItemResponse> Items,
        int TotalCount,
        int PageNumber,
        int PageSize);
}