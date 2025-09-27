using StreetRacer.Application.DTOs;

namespace StreetRacer.Application.Services;

public interface IRaceService
{
    Task<RaceDto> CreateRaceAsync(Guid hostId, CreateRaceDto createRaceDto);
    Task<RaceDto?> GetRaceAsync(Guid raceId);
    Task InviteRacersAsync(Guid raceId, Guid inviterId, ICollection<Guid> racerUserIds);
    Task AcceptInviteAsync(Guid raceId, Guid userId);
    Task StartRaceAsync(Guid raceId, Guid byUserId);
    Task CancelRaceAsync(Guid raceId, Guid byUserId);
    Task SubmitLocationAsync(Guid raceId, Guid userId, LocationDto locationDto);
    Task<RaceLiveStatusDto?> GetLiveStatusAsync(Guid raceId);
    Task<RaceLiveStatusDto?> GetResultsAsync(Guid raceId);
    Task FinishRaceAsync(Guid raceId);
    Task BroadcastRaceUpdateAsync(Guid raceId, RaceLiveStatusDto status);
}