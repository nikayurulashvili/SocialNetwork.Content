using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Entities.Entities;


namespace SocialNetwork.Content.Services.Services;


public class UserEventsService : IUserEventsService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserEventsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task HandleUserCreatedAsync(Guid messageId, Guid userId, bool isActive, CancellationToken ct = default)
    {
        return ApplyUserStateAsync(messageId, userId, isActive, ct);
    }

    public Task HandleUserUpdatedAsync(Guid messageId, Guid userId, bool isActive, CancellationToken ct = default)
    {
        return ApplyUserStateAsync(messageId, userId, isActive, ct);
    }

    public Task HandleUserDeletedAsync(Guid messageId, Guid userId, CancellationToken ct = default)
    {
        return ApplyUserStateAsync(messageId, userId, false, ct);
    }

    private async Task ApplyUserStateAsync(Guid messageId, Guid userId, bool isActive, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Inbox idempotency check and replica update are kept in one transaction.
            if (await _unitOfWork.InboxMessages.ExistsAsync(messageId))
            {
                await _unitOfWork.RollbackTransactionAsync();
                return;
            }

            await _unitOfWork.UserReplicas.AddOrUpdateAsync(new UserReplica
            {
                UserId = userId,
                IsActive = isActive
            });

            await _unitOfWork.InboxMessages.AddAsync(new InboxMessage
            {
                MessageId = messageId,
                ProcessedAt = DateTime.UtcNow
            });

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
