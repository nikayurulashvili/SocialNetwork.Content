using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
    public CommentRepository(ContentDbContext context) : base(context) { }
    public async Task<List<Comment>> GetCommentsByPostIdAsync(Guid postId) =>
        await _dbSet.AsNoTracking().Where(c => c.PostId == postId && !c.IsDeleted).OrderBy(c => c.CreatedAt).ToListAsync();
    public async Task<List<Comment>> GetCommentsByAuthorAsync(Guid authorId) =>
        await _dbSet.AsNoTracking().Where(c => c.AuthorId == authorId && !c.IsDeleted).OrderByDescending(c => c.CreatedAt).ToListAsync();
    public async Task SoftDeleteAsync(Guid commentId)
    {
        var comment = await _dbSet.FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment is not null) { comment.IsDeleted = true; comment.UpdatedAt = DateTime.UtcNow; }
    }
}