namespace SocialNetwork.EventBus.Outbox;

public interface IOutboxStore
{
    Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int take, CancellationToken ct);
    void MarkPublished(Guid id);
    void MarkFailed(Guid id, string error);
    Task SaveChangesAsync(CancellationToken ct);
}
