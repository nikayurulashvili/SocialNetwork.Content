using Microsoft.EntityFrameworkCore;
using SocialNetwork.Content.Entities.Entities;
using SocialNetwork.EventBus.Outbox;
using SocialNetwork.Content.Repositories.Persistence;

namespace SocialNetwork.Content.Repositories.Persistence.EventBus;

/// <summary>Adapts Content's EF Outbox table to the EventBus publisher contract.</summary>
public sealed class ContentOutboxStore(ContentDbContext db) : IOutboxStore
{
    public async Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int take, CancellationToken ct)
    {
        return await db.OutboxMessages
            .AsNoTracking()
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Take(take)
            .Select(x => new OutboxEnvelope(x.Id, x.Type, x.Payload))
            .ToListAsync(ct);
    }

    public void MarkPublished(Guid id)
    {
        var entity = db.OutboxMessages.Local.FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            entity = new OutboxMessage { Id = id };
            db.Attach(entity);
        }
        entity.ProcessedAt = DateTime.UtcNow;
        entity.LastError = null;
        db.Entry(entity).Property(x => x.ProcessedAt).IsModified = true;
        db.Entry(entity).Property(x => x.LastError).IsModified = true;
    }

    public void MarkFailed(Guid id, string error)
    {
        var entity = db.OutboxMessages.Local.FirstOrDefault(x => x.Id == id);
        if (entity is null)
        {
            entity = new OutboxMessage { Id = id };
            db.Attach(entity);
        }
        entity.LastError = error.Length > 2000 ? error[..2000] : error;
        db.Entry(entity).Property(x => x.LastError).IsModified = true;
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
