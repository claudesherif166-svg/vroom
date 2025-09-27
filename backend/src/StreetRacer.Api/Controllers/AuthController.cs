using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("sync")]
    [Authorize]
    public async Task<ActionResult<UserDto>> SyncUser()
    {
        var keycloakSub = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        var username = User.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrEmpty(keycloakSub) || string.IsNullOrEmpty(email))
        {
            return BadRequest("Invalid token claims");
        }

        var user = await _authService.SyncUserFromKeycloakAsync(keycloakSub, email, username ?? email);
        return Ok(user);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var keycloakSub = User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(keycloakSub))
        {
            return BadRequest("Invalid token");
        }

        var user = await _authService.GetUserByKeycloakSubAsync(keycloakSub);
        
        if (user == null)
        {
            return NotFound("User not found. Please sync first.");
        }

        return Ok(user);
    }
}