namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileProcessingOutboxDispatcher
    {
        Task DispatchPendingAsync(
            int batchSize,
            CancellationToken cancellationToken = default);
    }
}