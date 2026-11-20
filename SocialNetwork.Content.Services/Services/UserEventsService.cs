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

    public async Task HandleUserDeletedAsync(Guid messageId, Guid userId, CancellationToken ct = default)
    {
        if (await _unitOfWork.InboxMessages.ExistsAsync(messageId))
        {
            return;
        }

        // Deleted event may arrive before UserCreated; keep an inactive replica either way.
        await _unitOfWork.UserReplicas.AddOrUpdateAsync(new UserReplica
        {
            UserId = userId,
            IsActive = false
        });

        await _unitOfWork.InboxMessages.AddAsync(new InboxMessage
        {
            MessageId = messageId,
            ProcessedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task ApplyUserStateAsync(Guid messageId, Guid userId, bool isActive, CancellationToken ct)
    {
        if (await _unitOfWork.InboxMessages.ExistsAsync(messageId))
        {
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

        await _unitOfWork.SaveChangesAsync();
    }
}
