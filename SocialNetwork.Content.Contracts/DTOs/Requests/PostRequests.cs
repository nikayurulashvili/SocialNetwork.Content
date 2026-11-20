namespace SocialNetwork.Content.Services.DTOs.Requests;
public class CreatePostRequest
{
    public string Text { get; set; } = string.Empty;
    public byte Visibility { get; set; }
    public List<Guid> AudienceUserIds { get; set; } = new();
}
public class UpdatePostRequest { public string Text { get; set; } = string.Empty; }
public class UpdatePostAudienceRequest { public List<Guid> AudienceUserIds { get; set; } = new(); }