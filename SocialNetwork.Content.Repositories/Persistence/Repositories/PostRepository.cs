using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(ContentDbContext context) : base(context) { }
    public async Task<Post?> GetPostWithAudienceAsync(Guid postId) =>
        await _dbSet.Include(p => p.Audiences).FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);
    public async Task<List<Post>> GetPostsByAuthorAsync(Guid authorId) =>
        await _dbSet.AsNoTracking().Where(p => p.AuthorId == authorId && !p.IsDeleted).OrderByDescending(p => p.CreatedAt).ToListAsync();
    public async Task<List<Post>> GetFeedAsync(Guid viewerId, int skip, int take) =>
        await _dbSet.AsNoTracking().Where(p => !p.IsDeleted &&
            (p.Visibility == 0 || p.AuthorId == viewerId || p.Audiences.Any(a => a.UserId == viewerId)))
            .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).Skip(skip).Take(take).ToListAsync();
    public async Task SoftDeleteAsync(Guid postId)
    {
        var post = await _dbSet.FirstOrDefaultAsync(p => p.Id == postId);
        if (post is not null) { post.IsDeleted = true; post.UpdatedAt = DateTime.UtcNow; }
    }
}