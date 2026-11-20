namespace SocialNetwork.Content.Contracts.Events;
public sealed record UserDeletedEvent
{
    public Guid UserId { get; init; }
}