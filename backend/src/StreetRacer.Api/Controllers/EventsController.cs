using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EventDto>>> GetEvents([FromQuery] string? cursor = null)
    {
        var cursorObj = Cursor.FromBase64(cursor);
        var events = await _eventService.GetEventsAsync(cursorObj);
        return Ok(events);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createEventDto)
    {
        var userId = GetCurrentUserId();
        var eventDto = await _eventService.CreateEventAsync(userId, createEventDto);
        return CreatedAtAction(nameof(GetEvent), new { id = eventDto.Id }, eventDto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDto>> GetEvent(Guid id)
    {
        var eventDto = await _eventService.GetEventAsync(id);
        if (eventDto == null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventDto>> UpdateEvent(Guid id, [FromBody] UpdateEventDto updateEventDto)
    {
        var userId = GetCurrentUserId();
        var eventDto = await _eventService.UpdateEventAsync(id, userId, updateEventDto);
        return Ok(eventDto);
    }

    [HttpPost("{id:guid}/contributors")]
    public async Task<IActionResult> AddContributor(Guid id, [FromBody] AddContributorDto dto)
    {
        var userId = GetCurrentUserId();
        await _eventService.AddContributorAsync(id, userId, dto.UserId, dto.Role);
        return NoContent();
    }

    [HttpDelete("{id:guid}/contributors/{contributorId:guid}")]
    public async Task<IActionResult> RemoveContributor(Guid id, Guid contributorId)
    {
        var userId = GetCurrentUserId();
        await _eventService.RemoveContributorAsync(id, userId, contributorId);
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

public class AddContributorDto
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "contributor";
}