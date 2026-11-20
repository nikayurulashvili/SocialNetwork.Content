using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Abstractions.Persistence;
public interface IPostRepository : IBaseRepository<Post>
{
    Task<Post?> GetPostWithAudienceAsync(Guid postId);
    Task<List<Post>> GetPostsByAuthorAsync(Guid authorId);
    Task<List<Post>> GetFeedAsync(Guid viewerId, int skip, int take);
    Task SoftDeleteAsync(Guid postId);
}