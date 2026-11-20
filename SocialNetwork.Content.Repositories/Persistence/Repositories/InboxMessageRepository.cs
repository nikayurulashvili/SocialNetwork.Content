using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class InboxMessageRepository : IInboxMessageRepository
{
    private readonly ContentDbContext _context;
    public InboxMessageRepository(ContentDbContext context) { _context = context; }
    public async Task<bool> ExistsAsync(Guid messageId) => await _context.InboxMessages.AnyAsync(x => x.MessageId == messageId);
    public async Task AddAsync(InboxMessage message) => await _context.InboxMessages.AddAsync(message);
}