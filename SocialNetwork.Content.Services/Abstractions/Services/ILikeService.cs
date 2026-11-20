using SocialNetwork.Content.Services.DTOs.Responses;
namespace SocialNetwork.Content.Services.Abstractions.Services;
public interface ILikeService
{
    Task<LikeResponse> LikePostAsync(Guid postId, Guid userId);
    Task<LikeResponse> UnlikePostAsync(Guid postId, Guid userId);
}