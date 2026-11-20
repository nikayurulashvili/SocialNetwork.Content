using SocialNetwork.Content.Services.DTOs.Requests;
using SocialNetwork.Content.Services.DTOs.Responses;
namespace SocialNetwork.Content.Services.Abstractions.Services;
public interface IPostService
{
    Task<PostResponse> CreatePostAsync(Guid authorId, CreatePostRequest request);
    Task<PostResponse> UpdatePostAsync(Guid postId, Guid authorId, UpdatePostRequest request);
    Task DeletePostAsync(Guid postId, Guid authorId);
    Task<PostResponse> GetPostByIdAsync(Guid postId, Guid viewerId);
    Task<List<PostResponse>> GetPostsByAuthorAsync(Guid authorId);
    Task<List<PostResponse>> GetFeedAsync(Guid viewerId, int page, int pageSize);
}