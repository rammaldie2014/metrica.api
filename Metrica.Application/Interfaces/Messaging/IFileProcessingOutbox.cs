using Metrica.Application.Messaging.Contracts;

namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileProcessingOutbox
    {
        Task AddAsync(
            ProcessFileRequested message,
            CancellationToken cancellationToken = default);
    }
}