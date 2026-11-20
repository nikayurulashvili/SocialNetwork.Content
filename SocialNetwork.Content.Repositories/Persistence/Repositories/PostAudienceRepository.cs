using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class PostAudienceRepository : BaseRepository<PostAudience>, IPostAudienceRepository
{
    public PostAudienceRepository(ContentDbContext context) : base(context) { }
    public override Task<PostAudience?> GetByIdAsync(Guid id) =>
        throw new NotSupportedException("PostAudience has a composite key (PostId, UserId). Use composite-key methods.");
    public async Task<List<PostAudience>> GetAudienceByPostIdAsync(Guid postId) => await _dbSet.Where(x => x.PostId == postId).ToListAsync();
    public async Task<List<PostAudience>> GetPostsByUserIdAsync(Guid userId) => await _dbSet.Where(x => x.UserId == userId).ToListAsync();
    public async Task RemoveAudienceByPostIdAsync(Guid postId) => _dbSet.RemoveRange(await _dbSet.Where(x => x.PostId == postId).ToListAsync());
}