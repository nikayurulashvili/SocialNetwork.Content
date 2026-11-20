using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Abstractions.Persistence;
public interface IUserReplicaRepository
{
    Task<UserReplica?> GetByIdAsync(Guid userId);
    Task AddOrUpdateAsync(UserReplica user);
}