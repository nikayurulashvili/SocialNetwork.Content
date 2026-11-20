using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class UserReplicaRepository : IUserReplicaRepository
{
    private readonly ContentDbContext _context;
    public UserReplicaRepository(ContentDbContext context) { _context = context; }
    public async Task<UserReplica?> GetByIdAsync(Guid userId) => await _context.UserReplicas.FirstOrDefaultAsync(x => x.UserId == userId);
    public async Task AddOrUpdateAsync(UserReplica user)
    {
        var existing = await GetByIdAsync(user.UserId);
        if (existing is null) await _context.UserReplicas.AddAsync(user);
        else existing.IsActive = user.IsActive;
    }
}