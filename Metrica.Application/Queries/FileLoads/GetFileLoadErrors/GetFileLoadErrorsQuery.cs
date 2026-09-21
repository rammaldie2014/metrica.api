namespace Metrica.Application.Queries.FileLoads.GetFileLoadErrors
{
    public sealed record GetFileLoadErrorsQuery(
        long FileLoadId,
        int PageNumber = 1,
        int PageSize = 20);
}