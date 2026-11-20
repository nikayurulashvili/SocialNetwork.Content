namespace SocialNetwork.Content.Contracts.Events;
public sealed record UserCreatedEvent
{
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }
}