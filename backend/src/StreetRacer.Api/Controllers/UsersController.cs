using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetUserAsync(userId);
        
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfileDto>> GetUser(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var user = await _userService.GetUserProfileAsync(id, currentUserId);
        
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UpdateUserDto updateUserDto)
    {
        var userId = GetCurrentUserId();
        var user = await _userService.UpdateUserAsync(userId, updateUserDto);
        return Ok(user);
    }

    [HttpGet("{id:guid}/followers")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetFollowers(Guid id, [FromQuery] string? cursor = null)
    {
        var cursorObj = Cursor.FromBase64(cursor);
        var followers = await _userService.GetFollowersAsync(id, cursorObj);
        return Ok(followers);
    }

    [HttpGet("{id:guid}/following")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetFollowing(Guid id, [FromQuery] string? cursor = null)
    {
        var cursorObj = Cursor.FromBase64(cursor);
        var following = await _userService.GetFollowingAsync(id, cursorObj);
        return Ok(following);
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