
namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileProcessingConsumer
    {
        Task ConsumeAsync(
            CancellationToken cancellationToken = default);
    }
}
