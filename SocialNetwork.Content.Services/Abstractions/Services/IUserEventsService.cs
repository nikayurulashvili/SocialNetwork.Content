namespace SocialNetwork.Content.Services.Abstractions.Services;

public interface IUserEventsService
{
    Task HandleUserCreatedAsync(Guid messageId, Guid userId, bool isActive, CancellationToken ct = default);
    Task HandleUserUpdatedAsync(Guid messageId, Guid userId, bool isActive, CancellationToken ct = default);
    Task HandleUserDeletedAsync(Guid messageId, Guid userId, CancellationToken ct = default);
}
