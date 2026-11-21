using SocialNetwork.Content.Contracts.Events;
using SocialNetwork.Content.Entities;
using SocialNetwork.Content.Entities.Entities;
using SocialNetwork.Content.Services.Abstractions.Persistence;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.DTOs.Requests;
using SocialNetwork.Content.Services.DTOs.Responses;
using SocialNetwork.Content.Services.Exceptions;
using System.Text.Json;

namespace SocialNetwork.Content.Services.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CommentResponse> CreateCommentAsync(
        Guid postId,
        Guid authorId,
        CreateCommentRequest request)
    {
        ValidateText(request.Text);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var post = await _unitOfWork.Posts
                .GetPostWithAudienceAsync(postId)
                ?? throw new NotFoundException("Post not found.");

            if (post.IsDeleted)
                throw new NotFoundException("Post not found.");

            var author = await _unitOfWork.UserReplicas
                .GetByIdAsync(authorId);

            if (author is null || !author.IsActive)
                throw new ForbiddenException("User is not active.");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                AuthorId = authorId,
                Text = request.Text.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Comments.AddAsync(comment);

            await AddOutboxAsync(
                "comment.created",
                new CommentCreatedEvent
                {
                    CommentId = comment.Id,
                    PostId = postId,
                    AuthorId = authorId,
                    Text = comment.Text,
                    CreatedAt = comment.CreatedAt
                });

            await _unitOfWork.CommitTransactionAsync();

            return Map(comment);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<CommentResponse> UpdateCommentAsync(
        Guid commentId,
        Guid authorId,
        UpdateCommentRequest request)
    {
        ValidateText(request.Text);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var comment = await _unitOfWork.Comments
                .GetByIdAsync(commentId)
                ?? throw new NotFoundException("Comment not found.");

            if (comment.IsDeleted)
                throw new NotFoundException("Comment not found.");

            if (comment.AuthorId != authorId)
                throw new ForbiddenException(
                    "Only comment author can update it.");

            comment.Text = request.Text.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            await AddOutboxAsync(
                "comment.updated",
                new CommentUpdatedEvent
                {
                    CommentId = comment.Id,
                    PostId = comment.PostId,
                    AuthorId = authorId,
                    Text = comment.Text,
                    UpdatedAt = comment.UpdatedAt.Value
                });

            await _unitOfWork.CommitTransactionAsync();

            return Map(comment);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task DeleteCommentAsync(
        Guid commentId,
        Guid authorId)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var comment = await _unitOfWork.Comments
                .GetByIdAsync(commentId)
                ?? throw new NotFoundException("Comment not found.");

            if (comment.IsDeleted)
                throw new NotFoundException("Comment not found.");

            if (comment.AuthorId != authorId)
                throw new ForbiddenException(
                    "Only comment author can delete it.");

            var deletedAt = DateTime.UtcNow;

            await _unitOfWork.Comments.SoftDeleteAsync(commentId);

            await AddOutboxAsync(
                "comment.deleted",
                new CommentDeletedEvent
                {
                    CommentId = comment.Id,
                    PostId = comment.PostId,
                    AuthorId = authorId,
                    DeletedAt = deletedAt
                });

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<List<CommentResponse>> GetCommentsByPostAsync(
        Guid postId)
    {
        var post = await _unitOfWork.Posts
            .GetPostWithAudienceAsync(postId);

        if (post is null || post.IsDeleted)
            throw new NotFoundException("Post not found.");

        var comments = await _unitOfWork.Comments
            .GetCommentsByPostIdAsync(postId);

        return comments
            .Select(Map)
            .ToList();
    }

    private async Task AddOutboxAsync<T>(
        string type,
        T evt)
    {
        await _unitOfWork.OutboxMessages.AddAsync(
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = type,
                Payload = JsonSerializer.Serialize(evt),
                OccurredAt = DateTime.UtcNow
            });
    }

    private static CommentResponse Map(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            PostId = comment.PostId,
            AuthorId = comment.AuthorId,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    private static void ValidateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > 500)
        {
            throw new RequestValidateException(
                "Comment text is required and cannot exceed 500 characters.");
        }
    }
}