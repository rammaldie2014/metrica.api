namespace Metrica.Application.Queries.FileLoads.GetFileLoadContent
{
    public sealed record GetFileLoadContentQuery(
        long FileLoadId,
        int PageNumber = 1,
        int PageSize = 20);
}