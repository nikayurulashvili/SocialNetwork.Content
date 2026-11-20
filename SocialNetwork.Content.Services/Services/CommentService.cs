using System.Text.Json;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.DTOs.Requests;
using SocialNetwork.Content.Services.DTOs.Responses;
using SocialNetwork.Content.Services.Exceptions;
using SocialNetwork.Content.Contracts.Events;
using SocialNetwork.Content.Entities.Entities;
namespace SocialNetwork.Content.Services.Services;
public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    public CommentService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }
    public async Task<CommentResponse> CreateCommentAsync(Guid postId, Guid authorId, CreateCommentRequest request)
    {
        ValidateText(request.Text);
        var post = await _unitOfWork.Posts.GetPostWithAudienceAsync(postId) ?? throw new NotFoundException("Post not found.");
        if (post.IsDeleted) throw new RequestValidateException("Cannot comment on a deleted post.");
        var author = await _unitOfWork.UserReplicas.GetByIdAsync(authorId);
        if (author is null || !author.IsActive) throw new ForbiddenException("User is not active.");
        var comment = new Comment { Id = Guid.NewGuid(), PostId = postId, AuthorId = authorId, Text = request.Text.Trim(), CreatedAt = DateTime.UtcNow };
        await _unitOfWork.Comments.AddAsync(comment);
        await AddOutboxAsync("comment.created", new CommentCreatedEvent { CommentId = comment.Id, PostId = postId, AuthorId = authorId, Text = comment.Text, CreatedAt = comment.CreatedAt });
        await _unitOfWork.SaveChangesAsync();
        return Map(comment);
    }
    public async Task<CommentResponse> UpdateCommentAsync(Guid commentId, Guid authorId, UpdateCommentRequest request)
    {
        ValidateText(request.Text);
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId) ?? throw new NotFoundException("Comment not found.");
        if (comment.IsDeleted) throw new NotFoundException("Comment not found.");
        if (comment.AuthorId != authorId) throw new ForbiddenException("Only the comment author can update it.");
        comment.Text = request.Text.Trim(); comment.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Comments.Update(comment);
        await AddOutboxAsync("comment.updated", new CommentUpdatedEvent { CommentId = comment.Id, PostId = comment.PostId, AuthorId = authorId, Text = comment.Text, UpdatedAt = comment.UpdatedAt.Value });
        await _unitOfWork.SaveChangesAsync();
        return Map(comment);
    }
    public async Task DeleteCommentAsync(Guid commentId, Guid authorId)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(commentId) ?? throw new NotFoundException("Comment not found.");
        if (comment.IsDeleted) throw new NotFoundException("Comment not found.");
        if (comment.AuthorId != authorId) throw new ForbiddenException("Only the comment author can delete it.");
        await _unitOfWork.Comments.SoftDeleteAsync(commentId);
        await AddOutboxAsync("comment.deleted", new CommentDeletedEvent { CommentId = comment.Id, PostId = comment.PostId, AuthorId = authorId, DeletedAt = DateTime.UtcNow });
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task<List<CommentResponse>> GetCommentsByPostAsync(Guid postId)
    {
        var post = await _unitOfWork.Posts.GetPostWithAudienceAsync(postId) ?? throw new NotFoundException("Post not found.");
        return (await _unitOfWork.Comments.GetCommentsByPostIdAsync(postId)).Select(Map).ToList();
    }
    private async Task AddOutboxAsync<T>(string type, T evt) => await _unitOfWork.OutboxMessages.AddAsync(new OutboxMessage { Id = Guid.NewGuid(), Type = type, Payload = JsonSerializer.Serialize(evt), OccurredAt = DateTime.UtcNow });
    private static CommentResponse Map(Comment comment) => new() { Id = comment.Id, PostId = comment.PostId, AuthorId = comment.AuthorId, Text = comment.Text, CreatedAt = comment.CreatedAt, UpdatedAt = comment.UpdatedAt };
    private static void ValidateText(string text) { if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > 500) throw new RequestValidateException("Comment text is required and cannot exceed 500 characters."); }
}