using RabbitMQ.Client;

namespace SocialNetwork.EventBus.Connection;

internal sealed class RabbitMqConnectionProvider(IConnectionFactory factory) : IRabbitMqConnectionProvider, IAsyncDisposable // amis gareshe DI ar daxuravs connection-s
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private volatile IConnection? _connection;
    public async Task<IConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _lock.WaitAsync(ct);

        try
        {
            _connection ??= await factory.CreateConnectionAsync(ct);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _lock.Dispose();
    }
}
