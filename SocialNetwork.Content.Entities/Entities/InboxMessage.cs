namespace SocialNetwork.Content.Entities.Entities;

public sealed class InboxMessage
{
    public Guid MessageId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
