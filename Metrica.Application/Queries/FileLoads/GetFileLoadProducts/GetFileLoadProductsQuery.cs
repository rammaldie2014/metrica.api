namespace Metrica.Application.Queries.FileLoads.GetFileLoadProducts
{
    public sealed record GetFileLoadProductsQuery(
        long FileLoadId,
        int PageNumber = 1,
        int PageSize = 20);
}