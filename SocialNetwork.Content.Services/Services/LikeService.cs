using System.Text.Json;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.DTOs.Responses;
using SocialNetwork.Content.Services.Exceptions;
using SocialNetwork.Content.Contracts.Events;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Services;
public class LikeService : ILikeService
{
    private readonly IUnitOfWork _unitOfWork;
    public LikeService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }
    public async Task<LikeResponse> LikePostAsync(Guid postId, Guid userId)
    {
        var post = await _unitOfWork.Posts.GetPostWithAudienceAsync(postId) ?? throw new NotFoundException("Post not found.");
        if (post.IsDeleted) throw new RequestValidateException("Cannot like a deleted post.");
        var user = await _unitOfWork.UserReplicas.GetByIdAsync(userId);
        if (user is null || !user.IsActive) throw new ForbiddenException("User is not active.");
        if (!await _unitOfWork.PostLikes.IsLikedByUserAsync(postId, userId))
        {
            await _unitOfWork.PostLikes.AddAsync(new PostLike { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
            await AddOutboxAsync("post.liked", new PostLikedEvent { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
            await _unitOfWork.SaveChangesAsync();
        }
        return new LikeResponse { PostId = postId, UserId = userId, IsLiked = true, LikesCount = await _unitOfWork.PostLikes.GetLikesCountAsync(postId) };
    }
    public async Task<LikeResponse> UnlikePostAsync(Guid postId, Guid userId)
    {
        if (await _unitOfWork.PostLikes.IsLikedByUserAsync(postId, userId))
        {
            await _unitOfWork.PostLikes.RemoveLikeAsync(postId, userId);
            await AddOutboxAsync("post.unliked", new PostUnlikedEvent { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
            await _unitOfWork.SaveChangesAsync();
        }
        return new LikeResponse { PostId = postId, UserId = userId, IsLiked = false, LikesCount = await _unitOfWork.PostLikes.GetLikesCountAsync(postId) };
    }
    private async Task AddOutboxAsync<T>(string type, T evt) => await _unitOfWork.OutboxMessages.AddAsync(new OutboxMessage { Id = Guid.NewGuid(), Type = type, Payload = JsonSerializer.Serialize(evt), OccurredAt = DateTime.UtcNow });
}