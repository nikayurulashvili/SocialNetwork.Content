using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Abstractions.Persistence;
public interface IPostLikeRepository : IBaseRepository<PostLike>
{
    Task<List<PostLike>> GetLikesByPostIdAsync(Guid postId);
    Task<bool> IsLikedByUserAsync(Guid postId, Guid userId);
    Task RemoveLikeAsync(Guid postId, Guid userId);
    Task<int> GetLikesCountAsync(Guid postId);
}