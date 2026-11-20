namespace SocialNetwork.Content.Contracts.Events;
public sealed record UserUpdatedEvent
{
    public Guid UserId { get; init; }
    public bool IsActive { get; init; }
}