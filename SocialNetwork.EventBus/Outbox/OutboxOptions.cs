namespace SocialNetwork.EventBus.Outbox;

public sealed class OutboxOptions
{
    public string Exchange { get; set; } = null!;
    public int BatchSize { get; set; } = 50;
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);
}
