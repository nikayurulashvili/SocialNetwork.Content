using Microsoft.EntityFrameworkCore.Storage;
using SocialNetwork.Content.Services.Abstractions.Persistence;

namespace SocialNetwork.Content.Repositories.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ContentDbContext _context;

    private IDbContextTransaction? _transaction;

    private readonly Lazy<IPostRepository> _posts;
    private readonly Lazy<IPostAudienceRepository> _postAudiences;
    private readonly Lazy<ICommentRepository> _comments;
    private readonly Lazy<IPostLikeRepository> _postLikes;
    private readonly Lazy<IUserReplicaRepository> _userReplicas;
    private readonly Lazy<IInboxMessageRepository> _inboxMessages;
    private readonly Lazy<IOutboxMessageRepository> _outboxMessages;

    public IPostRepository Posts => _posts.Value;
    public IPostAudienceRepository PostAudiences => _postAudiences.Value;
    public ICommentRepository Comments => _comments.Value;
    public IPostLikeRepository PostLikes => _postLikes.Value;
    public IUserReplicaRepository UserReplicas => _userReplicas.Value;
    public IInboxMessageRepository InboxMessages => _inboxMessages.Value;
    public IOutboxMessageRepository OutboxMessages => _outboxMessages.Value;

    public UnitOfWork(ContentDbContext context)
    {
        _context = context;

        _posts = new Lazy<IPostRepository>(
            () => new PostRepository(_context));

        _postAudiences = new Lazy<IPostAudienceRepository>(
            () => new PostAudienceRepository(_context));

        _comments = new Lazy<ICommentRepository>(
            () => new CommentRepository(_context));

        _postLikes = new Lazy<IPostLikeRepository>(
            () => new PostLikeRepository(_context));

        _userReplicas = new Lazy<IUserReplicaRepository>(
            () => new UserReplicaRepository(_context));

        _inboxMessages = new Lazy<IInboxMessageRepository>(
            () => new InboxMessageRepository(_context));

        _outboxMessages = new Lazy<IOutboxMessageRepository>(
            () => new OutboxMessageRepository(_context));
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
            throw new InvalidOperationException(
                "A transaction is already active.");

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException(
                "No active transaction.");

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await _transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
            return;

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}