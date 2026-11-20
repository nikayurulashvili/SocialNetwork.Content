using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Abstractions.Persistence;
public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message);
}
