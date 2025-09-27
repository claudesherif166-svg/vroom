using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/races")]
[Authorize]
public class RacesController : ControllerBase
{
    private readonly IRaceService _raceService;
    private readonly ILogger<RacesController> _logger;

    public RacesController(IRaceService raceService, ILogger<RacesController> logger)
    {
        _raceService = raceService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<RaceDto>> CreateRace([FromBody] CreateRaceDto createRaceDto)
    {
        var userId = GetCurrentUserId();
        var race = await _raceService.CreateRaceAsync(userId, createRaceDto);
        return CreatedAtAction(nameof(GetRace), new { id = race.Id }, race);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RaceDto>> GetRace(Guid id)
    {
        var race = await _raceService.GetRaceAsync(id);
        if (race == null)
            return NotFound();

        return Ok(race);
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<IActionResult> InviteRacers(Guid id, [FromBody] InviteRacersDto dto)
    {
        var userId = GetCurrentUserId();
        await _raceService.InviteRacersAsync(id, userId, dto.RacerUserIds);
        return NoContent();
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> AcceptInvite(Guid id)
    {
        var userId = GetCurrentUserId();
        await _raceService.AcceptInviteAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> StartRace(Guid id)
    {
        var userId = GetCurrentUserId();
        await _raceService.StartRaceAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelRace(Guid id)
    {
        var userId = GetCurrentUserId();
        await _raceService.CancelRaceAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/location")]
    public async Task<IActionResult> SubmitLocation(Guid id, [FromBody] LocationDto locationDto)
    {
        var userId = GetCurrentUserId();
        await _raceService.SubmitLocationAsync(id, userId, locationDto);
        return NoContent();
    }

    [HttpGet("{id:guid}/live")]
    public async Task<ActionResult<RaceLiveStatusDto>> GetLiveStatus(Guid id)
    {
        var status = await _raceService.GetLiveStatusAsync(id);
        if (status == null)
            return NotFound();

        return Ok(status);
    }

    [HttpGet("{id:guid}/results")]
    public async Task<ActionResult<RaceLiveStatusDto>> GetResults(Guid id)
    {
        var results = await _raceService.GetResultsAsync(id);
        if (results == null)
            return NotFound();

        return Ok(results);
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

public class InviteRacersDto
{
    public ICollection<Guid> RacerUserIds { get; set; } = new List<Guid>();
}