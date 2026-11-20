using RabbitMQ.Client;

namespace SocialNetwork.EventBus.Connection;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync(CancellationToken ct);
}