namespace Metrica.Application.Queries.FileLoads.GetFileLoadProducts
{
    public sealed record GetFileLoadProductsItemResponse(
        long Id,
        long FileLoadId,
        string Period,
        string ProductCode,
        string ProductName,
        string Description,
        decimal Price,
        int Stock,
        DateTime CreatedAt);
}