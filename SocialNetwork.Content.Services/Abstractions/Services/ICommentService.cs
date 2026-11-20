using SocialNetwork.Content.Services.DTOs.Requests;
using SocialNetwork.Content.Services.DTOs.Responses;
namespace SocialNetwork.Content.Services.Abstractions.Services;
public interface ICommentService
{
    Task<CommentResponse> CreateCommentAsync(Guid postId, Guid authorId, CreateCommentRequest request);
    Task<CommentResponse> UpdateCommentAsync(Guid commentId, Guid authorId, UpdateCommentRequest request);
    Task DeleteCommentAsync(Guid commentId, Guid authorId);
    Task<List<CommentResponse>> GetCommentsByPostAsync(Guid postId);
}