using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Entities.Entities;
using Microsoft.EntityFrameworkCore;
namespace SocialNetwork.Content.Repositories.Persistence.Repositories;
public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly ContentDbContext _context;
    public OutboxMessageRepository(ContentDbContext context) { _context = context; }
    public async Task AddAsync(OutboxMessage message) => await _context.OutboxMessages.AddAsync(message);
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int take, int maxAttempts, CancellationToken ct= default)
    {
        return await _context.OutboxMessages
            .AsNoTracking()
            .Where(x => x.ProcessedAt == null && x.Attempts < maxAttempts)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(ct);
    }
    
      

    
}