namespace SocialNetwork.Content.Contracts.Events;
public sealed record CommentCreatedEvent
{
    public Guid CommentId { get; init; }
    public Guid PostId { get; init; }
    public Guid AuthorId { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}