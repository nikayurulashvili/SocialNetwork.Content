using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Content.Services.Abstractions.Services;
namespace SocialNetwork.Content.Api.Controllers;
[ApiController]
[Route("api/v1/posts/{postId:guid}/likes")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;
    public LikesController(ILikeService likeService) { _likeService = likeService; }
    [HttpPost]
    public async Task<IActionResult> Like(Guid postId) => Ok(await _likeService.LikePostAsync(postId, GetCurrentUserId()));
    [HttpDelete]
    public async Task<IActionResult> Unlike(Guid postId) => Ok(await _likeService.UnlikePostAsync(postId, GetCurrentUserId()));
    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var id)) throw new UnauthorizedAccessException("Authenticated user ID is missing.");
        return id;
    }
}