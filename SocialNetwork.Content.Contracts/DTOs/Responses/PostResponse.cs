namespace SocialNetwork.Content.Services.DTOs.Responses;
public class PostResponse
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string Text { get; set; } = string.Empty;
    public byte Visibility { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LikesCount { get; set; }
}