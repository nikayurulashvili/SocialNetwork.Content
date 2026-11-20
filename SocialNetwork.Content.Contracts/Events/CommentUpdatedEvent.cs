namespace SocialNetwork.Content.Contracts.Events;
public sealed record CommentUpdatedEvent
{
    public Guid CommentId { get; init; }
    public Guid PostId { get; init; }
    public Guid AuthorId { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}