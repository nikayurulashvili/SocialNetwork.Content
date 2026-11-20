using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Contracts.Events;
using SocialNetwork.EventBus.Connection;
using SocialNetwork.EventBus.Consuming;

namespace SocialNetwork.Content.Repositories.EventBus;

public class UserEventsConsumer : RabbitMqConsumer
{
    public const string DefaultExchange = "socialnetwork.events";
    public const string DefaultQueue = "content.user-events";

    public UserEventsConsumer(
        IRabbitMqConnectionProvider connections,
        IServiceScopeFactory scopeFactory,
        ILogger<UserEventsConsumer> logger)
        : base(connections, scopeFactory, logger)
    {
    }

    protected override string QueueName => DefaultQueue;
    protected override string Exchange => DefaultExchange;
    protected override IReadOnlyCollection<string> RoutingKeys =>
        new[] { "user.created", "user.updated", "user.deleted" };

    protected override async Task HandleAsync(
        IServiceProvider services,
        Guid messageId,
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken ct)
    {
        var userEvents = services.GetRequiredService<IUserEventsService>();

        switch (routingKey)
        {
            case "user.created":
            {
                var message = Deserialize<UserCreatedEvent>(body);
                await userEvents.HandleUserCreatedAsync(messageId, message.UserId, message.IsActive, ct);
                break;
            }
            case "user.updated":
            {
                var message = Deserialize<UserUpdatedEvent>(body);
                await userEvents.HandleUserUpdatedAsync(messageId, message.UserId, message.IsActive, ct);
                break;
            }
            case "user.deleted":
            {
                var message = Deserialize<UserDeletedEvent>(body);
                await userEvents.HandleUserDeletedAsync(messageId, message.UserId, ct);
                break;
            }
            default:
                throw new InvalidOperationException($"Unsupported user event routing key: {routingKey}.");
        }
    }
}
