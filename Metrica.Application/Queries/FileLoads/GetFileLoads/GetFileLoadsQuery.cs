namespace Metrica.Application.Queries.FileLoads.GetFileLoads
{
    public sealed record GetFileLoadsQuery(
        int PageNumber = 1,
        int PageSize = 20);
}