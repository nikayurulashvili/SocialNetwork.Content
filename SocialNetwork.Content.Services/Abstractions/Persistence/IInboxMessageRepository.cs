using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Abstractions.Persistence;
public interface IInboxMessageRepository
{
    Task<bool> ExistsAsync(Guid messageId);
    Task AddAsync(InboxMessage message);
}