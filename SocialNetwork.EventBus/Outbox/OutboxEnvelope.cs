namespace SocialNetwork.EventBus.Outbox;

public sealed record OutboxEnvelope(Guid Id, string Type, string Payload);
