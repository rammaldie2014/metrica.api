namespace Metrica.Application.Queries.FileLoads.GetFileLoadErrors
{
    public sealed record GetFileLoadErrorsItemResponse(
        long Id,
        long FileLoadId,
        int? RowNumber,
        string Code,
        string Message,
        DateTime CreatedAt);
}