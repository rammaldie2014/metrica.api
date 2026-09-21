
namespace Metrica.Application.Interfaces.Storage
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(
            Stream content,
            string fileName,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default);

        Task<Stream> OpenReadAsync(
            string filePath,
            CancellationToken cancellationToken = default);

    }
}
