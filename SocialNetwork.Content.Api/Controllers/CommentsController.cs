using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Content.Services.Abstractions.Services;
using SocialNetwork.Content.Services.DTOs.Requests;
namespace SocialNetwork.Content.Api.Controllers;
[ApiController]
[Route("api/v1")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    public CommentsController(ICommentService commentService) { _commentService = commentService; }
    [HttpPost("posts/{postId:guid}/comments")]
    public async Task<IActionResult> Create(Guid postId, [FromBody] CreateCommentRequest request) =>
        Ok(await _commentService.CreateCommentAsync(postId, GetCurrentUserId(), request));
    [HttpGet("posts/{postId:guid}/comments")]
    public async Task<IActionResult> GetByPost(Guid postId) => Ok(await _commentService.GetCommentsByPostAsync(postId));
    [HttpPut("comments/{commentId:guid}")]
    public async Task<IActionResult> Update(Guid commentId, [FromBody] UpdateCommentRequest request) =>
        Ok(await _commentService.UpdateCommentAsync(commentId, GetCurrentUserId(), request));
    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid commentId)
    {
        await _commentService.DeleteCommentAsync(commentId, GetCurrentUserId());
        return NoContent();
    }
    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var id)) throw new UnauthorizedAccessException("Authenticated user ID is missing.");
        return id;
    }
}