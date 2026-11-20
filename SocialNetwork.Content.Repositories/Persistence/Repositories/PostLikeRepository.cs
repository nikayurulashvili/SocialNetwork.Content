using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class PostLikeRepository : BaseRepository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(ContentDbContext context) : base(context) { }
    public override Task<PostLike?> GetByIdAsync(Guid id) =>
        throw new NotSupportedException("PostLike has a composite key (PostId, UserId). Use composite-key methods.");
    public async Task<List<PostLike>> GetLikesByPostIdAsync(Guid postId) => await _dbSet.Where(x => x.PostId == postId).ToListAsync();
    public async Task<bool> IsLikedByUserAsync(Guid postId, Guid userId) => await _dbSet.AnyAsync(x => x.PostId == postId && x.UserId == userId);
    public async Task RemoveLikeAsync(Guid postId, Guid userId)
    {
        var like = await _dbSet.FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == userId);
        if (like is not null) _dbSet.Remove(like);
    }
    public async Task<int> GetLikesCountAsync(Guid postId) => await _dbSet.CountAsync(x => x.PostId == postId);
}