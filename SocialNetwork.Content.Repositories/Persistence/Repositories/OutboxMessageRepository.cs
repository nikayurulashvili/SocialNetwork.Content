using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly ContentDbContext _context;
    public OutboxMessageRepository(ContentDbContext context) { _context = context; }
    public async Task AddAsync(OutboxMessage message) => await _context.OutboxMessages.AddAsync(message);
}