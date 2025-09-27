using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VehicleDto>>> GetVehicles([FromQuery] Guid? ownerId = null, [FromQuery] string? cursor = null)
    {
        var cursorObj = Cursor.FromBase64(cursor);
        var vehicles = await _vehicleService.GetVehiclesAsync(ownerId, cursorObj);
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> CreateVehicle([FromBody] CreateVehicleDto createVehicleDto)
    {
        var userId = GetCurrentUserId();
        var vehicle = await _vehicleService.CreateVehicleAsync(userId, createVehicleDto);
        return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> GetVehicle(Guid id)
    {
        var vehicle = await _vehicleService.GetVehicleAsync(id);
        if (vehicle == null)
            return NotFound();

        return Ok(vehicle);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> UpdateVehicle(Guid id, [FromBody] UpdateVehicleDto updateVehicleDto)
    {
        var userId = GetCurrentUserId();
        var vehicle = await _vehicleService.UpdateVehicleAsync(id, userId, updateVehicleDto);
        return Ok(vehicle);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        var userId = GetCurrentUserId();
        await _vehicleService.DeleteVehicleAsync(id, userId);
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