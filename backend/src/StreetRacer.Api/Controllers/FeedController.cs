using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/feed")]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly IFeedService _feedService;
    private readonly ILogger<FeedController> _logger;

    public FeedController(IFeedService feedService, ILogger<FeedController> logger)
    {
        _feedService = feedService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PostDto>>> GetFeed([FromQuery] string? cursor = null, [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        var cursorObj = Cursor.FromBase64(cursor);
        cursorObj.Limit = Math.Min(limit, 50); // Cap at 50
        
        var feed = await _feedService.GetPersonalizedFeedAsync(userId, cursorObj);
        return Ok(feed);
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