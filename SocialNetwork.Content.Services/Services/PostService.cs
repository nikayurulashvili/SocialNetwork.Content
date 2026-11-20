using System.Text.Json;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.DTOs.Requests;
using SocialNetwork.Content.Services.DTOs.Responses;
using SocialNetwork.Content.Services.Exceptions;
using SocialNetwork.Content.Contracts.Events;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Services;
public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    public PostService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }
    public async Task<PostResponse> CreatePostAsync(Guid authorId, CreatePostRequest request)
    {
        ValidateText(request.Text, 3000);
        if (request.Visibility > 1) throw new RequestValidateException("Visibility must be 0 or 1.");
        var author = await _unitOfWork.UserReplicas.GetByIdAsync(authorId);
        if (author is null || !author.IsActive) throw new ForbiddenException("User is not active or synchronized.");
        var audienceIds = request.AudienceUserIds.Distinct().ToList();
        if (request.Visibility == 1)
        {
            foreach (var userId in audienceIds)
            {
                var user = await _unitOfWork.UserReplicas.GetByIdAsync(userId);
                if (user is null || !user.IsActive) throw new RequestValidateException($"Audience user {userId} is not active.");
            }
        }
        else if (audienceIds.Count > 0) throw new RequestValidateException("Audience can only be specified for restricted posts.");
        var post = new Post { Id = Guid.NewGuid(), AuthorId = authorId, Text = request.Text.Trim(), Visibility = request.Visibility, CreatedAt = DateTime.UtcNow };
        await _unitOfWork.Posts.AddAsync(post);
        if (request.Visibility == 1)
            foreach (var userId in audienceIds) await _unitOfWork.PostAudiences.AddAsync(new PostAudience { PostId = post.Id, UserId = userId });
        var evt = new PostCreatedEvent { PostId = post.Id, AuthorId = authorId, Text = post.Text, Visibility = post.Visibility, AudienceUserIds = audienceIds, CreatedAt = post.CreatedAt };
        await AddOutboxAsync("post.created", evt);
        await _unitOfWork.SaveChangesAsync();
        return Map(post, 0);
    }
    public async Task<PostResponse> UpdatePostAsync(Guid postId, Guid authorId, UpdatePostRequest request)
    {
        ValidateText(request.Text, 3000);
        var post = await _unitOfWork.Posts.GetPostWithAudienceAsync(postId) ?? throw new NotFoundException("Post not found.");
        if (post.AuthorId != authorId) throw new ForbiddenException("Only the post author can update it.");
        post.Text = request.Text.Trim(); post.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Posts.Update(post);
        var evt = new PostUpdatedEvent { PostId = post.Id, AuthorId = post.AuthorId, Text = post.Text, Visibility = post.Visibility, AudienceUserIds = post.Audiences.Select(x => x.UserId).ToArray(), UpdatedAt = post.UpdatedAt.Value };
        await AddOutboxAsync("post.updated", evt);
        await _unitOfWork.SaveChangesAsync();
        return Map(post, await _unitOfWork.PostLikes.GetLikesCountAsync(post.Id));
    }
    public async Task DeletePostAsync(Guid postId, Guid authorId)
    {
        var post = await _unitOfWork.Posts.GetPostWithAudienceAsync(postId) ?? throw new NotFoundException("Post not found.");
        if (post.AuthorId != authorId) throw new ForbiddenException("Only the post author can delete it.");
        await _unitOfWork.Posts.SoftDeleteAsync(postId);
        var evt = new PostDeletedEvent { PostId = post.Id, AuthorId = authorId, DeletedAt = DateTime.UtcNow };
        await AddOutboxAsync("post.deleted", evt);
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task<PostResponse> GetPostByIdAsync(Guid postId, Guid viewerId)
    {
        var post = await _unitOfWork.Posts.GetPostWithAudienceAsync(postId) ?? throw new NotFoundException("Post not found.");
        if (post.Visibility == 1 && post.AuthorId != viewerId && !post.Audiences.Any(x => x.UserId == viewerId))
            throw new ForbiddenException("You cannot view this post.");
        return Map(post, await _unitOfWork.PostLikes.GetLikesCountAsync(post.Id));
    }
    public async Task<List<PostResponse>> GetPostsByAuthorAsync(Guid authorId)
    {
        var posts = await _unitOfWork.Posts.GetPostsByAuthorAsync(authorId);
        var result = new List<PostResponse>();
        foreach (var post in posts) result.Add(Map(post, await _unitOfWork.PostLikes.GetLikesCountAsync(post.Id)));
        return result;
    }
    public async Task<List<PostResponse>> GetFeedAsync(Guid viewerId, int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw new RequestValidateException("Invalid pagination values.");
        var posts = await _unitOfWork.Posts.GetFeedAsync(viewerId, (page - 1) * pageSize, pageSize);
        var result = new List<PostResponse>();
        foreach (var post in posts) result.Add(Map(post, await _unitOfWork.PostLikes.GetLikesCountAsync(post.Id)));
        return result;
    }
    private async Task AddOutboxAsync<T>(string type, T evt)
    {
        await _unitOfWork.OutboxMessages.AddAsync(new OutboxMessage { Id = Guid.NewGuid(), Type = type, Payload = JsonSerializer.Serialize(evt), OccurredAt = DateTime.UtcNow });
    }
    private static PostResponse Map(Post post, int likes) => new() { Id = post.Id, AuthorId = post.AuthorId, Text = post.Text, Visibility = post.Visibility, CreatedAt = post.CreatedAt, UpdatedAt = post.UpdatedAt, LikesCount = likes };
    private static void ValidateText(string text, int max) { if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > max) throw new RequestValidateException($"Text is required and must be at most {max} characters."); }
}