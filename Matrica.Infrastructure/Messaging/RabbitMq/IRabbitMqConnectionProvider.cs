using RabbitMQ.Client;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public interface IRabbitMqConnectionProvider : IAsyncDisposable
    {
        Task<IConnection> GetConnectionAsync(
            CancellationToken cancellationToken = default);
    }
}
