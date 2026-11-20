using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetwork.Content.Contracts.Events;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.EventBus.Connection;
using SocialNetwork.EventBus.Consuming;

namespace SocialNetwork.Content.Api.Consumers;
public sealed class UserDeletedConsumer(IRabbitMqConnectionProvider connections, IServiceScopeFactory scopes, ILogger<UserDeletedConsumer> logger)
    : RabbitMqConsumer(connections, scopes, logger)
{
    protected override string QueueName => "content.user-deleted";
    protected override string Exchange => "socialnetwork.events";
    protected override IReadOnlyCollection<string> RoutingKeys => ["user.deleted"];
    protected override async Task HandleAsync(IServiceProvider services, Guid messageId, string routingKey, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        var evt = Deserialize<UserDeletedEvent>(body);
        await services.GetRequiredService<IUserEventsService>().HandleUserDeletedAsync(messageId, evt.UserId, ct);
    }
}
