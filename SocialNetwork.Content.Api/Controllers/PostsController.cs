using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.DTOs.Requests;
namespace SocialNetwork.Content.Api.Controllers;
[ApiController]
[Route("api/v1/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    public PostsController(IPostService postService) { _postService = postService; }
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var authorId = GetCurrentUserId();
        var response = await _postService.CreatePostAsync(authorId, request);
        return CreatedAtAction(nameof(GetPost), new { postId = response.Id }, response);
    }
    [HttpGet("{postId:guid}")]
    public async Task<IActionResult> GetPost(Guid postId)
    {
        var viewerId = GetCurrentUserId();
        return Ok(await _postService.GetPostByIdAsync(postId, viewerId));
    }
    [HttpGet("author/{authorId:guid}")]
    public async Task<IActionResult> GetByAuthor(Guid authorId) => Ok(await _postService.GetPostsByAuthorAsync(authorId));
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        Ok(await _postService.GetFeedAsync(GetCurrentUserId(), page, pageSize));
    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> UpdatePost(Guid postId, [FromBody] UpdatePostRequest request) =>
        Ok(await _postService.UpdatePostAsync(postId, GetCurrentUserId(), request));
    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        await _postService.DeletePostAsync(postId, GetCurrentUserId());
        return NoContent();
    }
    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var id)) throw new UnauthorizedAccessException("Authenticated user ID is missing.");
        return id;
    }
}