using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/posts")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostService postService, ILogger<PostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto)
    {
        var userId = GetCurrentUserId();
        var post = await _postService.CreatePostAsync(userId, createPostDto);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var post = await _postService.GetPostAsync(id, currentUserId);
        
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var userId = GetCurrentUserId();
        await _postService.DeletePostAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/likes")]
    public async Task<IActionResult> LikePost(Guid id)
    {
        var userId = GetCurrentUserId();
        await _postService.LikePostAsync(id, userId);
        return NoContent();
    }

    [HttpDelete("{id:guid}/likes")]
    public async Task<IActionResult> UnlikePost(Guid id)
    {
        var userId = GetCurrentUserId();
        await _postService.UnlikePostAsync(id, userId);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}