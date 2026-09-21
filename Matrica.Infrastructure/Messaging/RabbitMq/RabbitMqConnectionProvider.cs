using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class RabbitMqConnectionProvider
         : IRabbitMqConnectionProvider
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private IConnection? _connection;
        private bool _disposed;

        public RabbitMqConnectionProvider(
            IOptions<RabbitMqOptions> options)
        {
            var settings = options.Value;

            _connectionFactory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
        }

        public async Task<IConnection> GetConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                ObjectDisposedException.ThrowIf(_disposed, this);

                if (_connection is null)
                {
                    _connection = await _connectionFactory
                        .CreateConnectionAsync(cancellationToken);
                }

                return _connection;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                if (_connection is not null)
                {
                    await _connection.DisposeAsync();
                    _connection = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
