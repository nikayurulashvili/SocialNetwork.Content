namespace SocialNetwork.Content.Contracts.Events;
public sealed record PostDeletedEvent
{
    public Guid PostId { get; init; }
    public Guid AuthorId { get; init; }
    public DateTime DeletedAt { get; init; }
}