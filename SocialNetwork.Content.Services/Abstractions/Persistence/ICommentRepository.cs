using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Abstractions.Persistence;
public interface ICommentRepository : IBaseRepository<Comment>
{
    Task<List<Comment>> GetCommentsByPostIdAsync(Guid postId);
    Task<List<Comment>> GetCommentsByAuthorAsync(Guid authorId);
    Task SoftDeleteAsync(Guid commentId);
}