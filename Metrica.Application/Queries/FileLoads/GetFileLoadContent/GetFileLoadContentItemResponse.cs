namespace Metrica.Application.Queries.FileLoads.GetFileLoadContent
{
    public sealed record GetFileLoadContentItemResponse(
        int RowNumber,
        string Period,
        string ProductCode,
        string ProductName,
        string Description,
        string Price,
        string Stock);
}