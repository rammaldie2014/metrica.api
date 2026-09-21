using Metrica.Application.Messaging.Contracts;

namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileLoadNotificationOutbox
    {
        Task AddAsync(
            FileLoadNotificationRequested message,
            CancellationToken cancellationToken = default);
    }
}