namespace SocialNetwork.Content.Contracts.Events;
public sealed record PostUnlikedEvent
{
    public Guid PostId { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}