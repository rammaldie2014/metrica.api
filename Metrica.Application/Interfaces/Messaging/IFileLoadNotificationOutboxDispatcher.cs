namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileLoadNotificationOutboxDispatcher
    {
        Task DispatchPendingAsync(
            int batchSize,
            CancellationToken cancellationToken = default);
    }
}