namespace Metrica.Application.Interfaces.Excel
{
    public interface IFileLoadPeriodResolver
    {
        string Resolve(
            Stream content,
            CancellationToken cancellationToken = default);
    }
}