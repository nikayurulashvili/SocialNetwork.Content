namespace SocialNetwork.Content.Services.Abstractions.Persistence;
public interface IUnitOfWork
{
    IPostRepository Posts { get; }
    IPostAudienceRepository PostAudiences { get; }
    ICommentRepository Comments { get; }
    IPostLikeRepository PostLikes { get; }
    IUserReplicaRepository UserReplicas { get; }
    IInboxMessageRepository InboxMessages { get; }
    IOutboxMessageRepository OutboxMessages { get; }
    Task<int> SaveChangesAsync();
}