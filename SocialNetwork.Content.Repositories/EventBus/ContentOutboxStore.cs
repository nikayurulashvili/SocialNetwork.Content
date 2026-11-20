using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Entities.Entities;
using SocialNetwork.Content.Repositories.Persistence;
using SocialNetwork.EventBus.Outbox;

namespace SocialNetwork.Content.Repositories.EventBus;

public class ContentOutboxStore : IOutboxStore
{
    private readonly ContentDbContext _context;
    private readonly Dictionary<Guid, OutboxMessage> _loadedMessages = new();

    public ContentOutboxStore(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int take, CancellationToken ct)
    {
        var messages = await _context.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .ToListAsync(ct);

        _loadedMessages.Clear();

        foreach (var message in messages)
        {
            _loadedMessages[message.Id] = message;
        }

        return messages
            .Select(x => new OutboxEnvelope(x.Id, x.Type, x.Payload))
            .ToArray();
    }

    public void MarkPublished(Guid id)
    {
        if (!_loadedMessages.TryGetValue(id, out var message))
        {
            return;
        }

        message.ProcessedAt = DateTime.UtcNow;
        message.LastError = null;
    }

    public void MarkFailed(Guid id, string error)
    {
        if (!_loadedMessages.TryGetValue(id, out var message))
        {
            return;
        }

        message.Attempts++;
        message.LastError = error;
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}
