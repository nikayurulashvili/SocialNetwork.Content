namespace SocialNetwork.Content.Contracts.Events;
public sealed record CommentDeletedEvent
{
    public Guid CommentId { get; init; }
    public Guid PostId { get; init; }
    public Guid AuthorId { get; init; }
    public DateTime DeletedAt { get; init; }
}