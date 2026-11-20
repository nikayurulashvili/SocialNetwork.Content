using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SocialNetwork.Content.Contracts.Events;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.EventBus.Connection;
using SocialNetwork.EventBus.Consuming;

namespace SocialNetwork.Content.Api.Consumers;
public sealed class UserUpdatedConsumer(IRabbitMqConnectionProvider connections, IServiceScopeFactory scopes, ILogger<UserUpdatedConsumer> logger)
    : RabbitMqConsumer(connections, scopes, logger)
{
    protected override string QueueName => "content.user-updated";
    protected override string Exchange => "socialnetwork.events";
    protected override IReadOnlyCollection<string> RoutingKeys => ["user.updated"];
    protected override async Task HandleAsync(IServiceProvider services, Guid messageId, string routingKey, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        var evt = Deserialize<UserUpdatedEvent>(body);
        await services.GetRequiredService<IUserEventsService>().HandleUserUpdatedAsync(messageId, evt.UserId, evt.IsActive, ct);
    }
}
