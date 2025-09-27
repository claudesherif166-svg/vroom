using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using System.Security.Claims;

namespace StreetRacer.Api.Hubs;

[Authorize]
public class RaceHub : Hub
{
    private readonly IRaceService _raceService;
    private readonly ILogger<RaceHub> _logger;

    public RaceHub(IRaceService raceService, ILogger<RaceHub> logger)
    {
        _raceService = raceService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} connected to RaceHub", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} disconnected from RaceHub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRace(string raceId)
    {
        var userId = GetCurrentUserId();
        
        if (!Guid.TryParse(raceId, out var raceGuid))
        {
            await Clients.Caller.SendAsync("Error", "Invalid race ID");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"race:{raceId}");
        _logger.LogInformation("User {UserId} joined race group {RaceId}", userId, raceId);
    }

    public async Task LeaveRace(string raceId)
    {
        var userId = GetCurrentUserId();
        
        if (!Guid.TryParse(raceId, out var raceGuid))
        {
            await Clients.Caller.SendAsync("Error", "Invalid race ID");
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"race:{raceId}");
        _logger.LogInformation("User {UserId} left race group {RaceId}", userId, raceId);
    }

    public async Task SendLocation(string raceId, LocationDto locationDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (!Guid.TryParse(raceId, out var raceGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid race ID");
                return;
            }

            // Submit location update (will be processed by worker)
            await _raceService.SubmitLocationAsync(raceGuid, userId, locationDto);
            
            _logger.LogDebug("Location update received from user {UserId} for race {RaceId}", 
                userId, raceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing location update for race {RaceId}", raceId);
            await Clients.Caller.SendAsync("Error", "Failed to process location update");
        }
    }

    public async Task RequestStart(string raceId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (!Guid.TryParse(raceId, out var raceGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid race ID");
                return;
            }

            await _raceService.StartRaceAsync(raceGuid, userId);
            
            // Notify all participants in the race
            await Clients.Group($"race:{raceId}").SendAsync("RaceStartRequested", new
            {
                RaceId = raceId,
                RequestedBy = userId,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation("Race start requested by user {UserId} for race {RaceId}", 
                userId, raceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting race start for race {RaceId}", raceId);
            await Clients.Caller.SendAsync("Error", "Failed to start race");
        }
    }

    public async Task AcceptInvite(string raceId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (!Guid.TryParse(raceId, out var raceGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid race ID");
                return;
            }

            await _raceService.AcceptInviteAsync(raceGuid, userId);
            
            // Notify all participants in the race
            await Clients.Group($"race:{raceId}").SendAsync("ParticipantJoined", new
            {
                RaceId = raceId,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation("User {UserId} accepted invite for race {RaceId}", 
                userId, raceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invite for race {RaceId}", raceId);
            await Clients.Caller.SendAsync("Error", "Failed to accept invite");
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }
}