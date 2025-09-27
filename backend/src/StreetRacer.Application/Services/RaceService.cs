using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Repositories;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StreetRacer.Api.Hubs;

namespace StreetRacer.Infrastructure.Services;

public class RaceService : IRaceService
{
    private readonly IRaceRepository _raceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDatabase _redis;
    private readonly IHubContext<RaceHub> _raceHubContext;
    private readonly ILogger<RaceService> _logger;
    private readonly GeometryFactory _geometryFactory;

    public RaceService(
        IRaceRepository raceRepository,
        IUserRepository userRepository,
        IConnectionMultiplexer redis,
        IHubContext<RaceHub> raceHubContext,
        ILogger<RaceService> logger)
    {
        _raceRepository = raceRepository;
        _userRepository = userRepository;
        _redis = redis.GetDatabase();
        _raceHubContext = raceHubContext;
        _logger = logger;
        
        var geometryServices = NtsGeometryServices.Instance;
        _geometryFactory = geometryServices.CreateGeometryFactory(srid: 4326);
    }

    public async Task<RaceDto> CreateRaceAsync(Guid hostId, CreateRaceDto createRaceDto)
    {
        var host = await _userRepository.GetAsync(hostId);
        if (host == null)
            throw new ArgumentException("Host user not found");

        var startPoint = _geometryFactory.CreatePoint(new Coordinate(createRaceDto.StartLongitude, createRaceDto.StartLatitude));
        var endPoint = _geometryFactory.CreatePoint(new Coordinate(createRaceDto.EndLongitude, createRaceDto.EndLatitude));

        var race = new Race
        {
            HostId = hostId,
            Name = createRaceDto.Name,
            StartPoint = startPoint,
            EndPoint = endPoint,
            ScheduledAt = createRaceDto.ScheduledAt,
            Status = "created"
        };

        // TODO: Convert RouteGeoJson to LineString if provided

        await _raceRepository.AddAsync(race);

        // Add racers
        var racerOrder = 1;
        foreach (var racerUserId in createRaceDto.RacerUserIds)
        {
            var racer = new RaceRacer
            {
                RaceId = race.Id,
                UserId = racerUserId,
                StartOrder = racerOrder++,
                Status = "invited"
            };
            await _raceRepository.AddRacerAsync(racer);
        }

        _logger.LogInformation("Race {RaceId} created by user {HostId}", race.Id, hostId);

        return await MapRaceToDto(race);
    }

    public async Task<RaceDto?> GetRaceAsync(Guid raceId)
    {
        var race = await _raceRepository.GetWithRacersAsync(raceId);
        if (race == null)
            return null;

        return await MapRaceToDto(race);
    }

    public async Task InviteRacersAsync(Guid raceId, Guid inviterId, ICollection<Guid> racerUserIds)
    {
        var race = await _raceRepository.GetAsync(raceId);
        if (race == null)
            throw new ArgumentException("Race not found");

        if (race.HostId != inviterId)
            throw new UnauthorizedAccessException("Only the host can invite racers");

        var existingRacers = await _raceRepository.GetRaceRacersAsync(raceId);
        var maxStartOrder = existingRacers.Any() ? existingRacers.Max(r => r.StartOrder) : 0;

        foreach (var racerUserId in racerUserIds)
        {
            if (!existingRacers.Any(r => r.UserId == racerUserId))
            {
                var racer = new RaceRacer
                {
                    RaceId = raceId,
                    UserId = racerUserId,
                    StartOrder = ++maxStartOrder,
                    Status = "invited"
                };
                await _raceRepository.AddRacerAsync(racer);
            }
        }

        _logger.LogInformation("Racers invited to race {RaceId} by user {InviterId}", raceId, inviterId);
    }

    public async Task AcceptInviteAsync(Guid raceId, Guid userId)
    {
        var racer = await _raceRepository.GetRaceRacerAsync(raceId, userId);
        if (racer == null)
            throw new ArgumentException("Racer not found in this race");

        if (racer.Status != "invited")
            throw new InvalidOperationException("Racer has already responded to the invite");

        racer.Status = "accepted";
        racer.JoinedAt = DateTime.UtcNow;

        await _raceRepository.UpdateRacerAsync(racer);

        _logger.LogInformation("User {UserId} accepted invite for race {RaceId}", userId, raceId);
    }

    public async Task StartRaceAsync(Guid raceId, Guid byUserId)
    {
        var race = await _raceRepository.GetWithRacersAsync(raceId);
        if (race == null)
            throw new ArgumentException("Race not found");

        if (race.HostId != byUserId)
            throw new UnauthorizedAccessException("Only the host can start the race");

        if (race.Status != "created")
            throw new InvalidOperationException("Race cannot be started in its current state");

        race.Status = "in_progress";
        race.StartedAt = DateTime.UtcNow;

        // Update all accepted racers to racing status
        foreach (var racer in race.Racers.Where(r => r.Status == "accepted"))
        {
            racer.Status = "racing";
        }

        await _raceRepository.UpdateAsync(race);

        // Initialize Redis ranking for this race
        await InitializeRaceRankingAsync(raceId);

        _logger.LogInformation("Race {RaceId} started by user {UserId}", raceId, byUserId);

        // Broadcast race start to all participants
        var status = await GetLiveStatusAsync(raceId);
        if (status != null)
        {
            await BroadcastRaceUpdateAsync(raceId, status);
        }
    }

    public async Task CancelRaceAsync(Guid raceId, Guid byUserId)
    {
        var race = await _raceRepository.GetAsync(raceId);
        if (race == null)
            throw new ArgumentException("Race not found");

        if (race.HostId != byUserId)
            throw new UnauthorizedAccessException("Only the host can cancel the race");

        race.Status = "cancelled";
        await _raceRepository.UpdateAsync(race);

        // Clean up Redis data
        await _redis.KeyDeleteAsync($"race:{raceId}:positions");

        _logger.LogInformation("Race {RaceId} cancelled by user {UserId}", raceId, byUserId);
    }

    public async Task SubmitLocationAsync(Guid raceId, Guid userId, LocationDto locationDto)
    {
        var race = await _raceRepository.GetAsync(raceId);
        if (race == null)
            throw new ArgumentException("Race not found");

        if (race.Status != "in_progress")
            throw new InvalidOperationException("Race is not in progress");

        var locationUpdate = new RacerLocationUpdate
        {
            RaceId = raceId,
            RacerUserId = userId,
            Latitude = locationDto.Latitude,
            Longitude = locationDto.Longitude,
            Speed = locationDto.Speed,
            Heading = locationDto.Heading,
            RecordedAt = locationDto.RecordedAt
        };

        // Basic anti-cheat validation
        var lastUpdate = await _raceRepository.GetLastLocationAsync(raceId, userId);
        if (lastUpdate != null)
        {
            var timeDiff = (locationUpdate.RecordedAt - lastUpdate.RecordedAt).TotalSeconds;
            if (timeDiff > 0)
            {
                var distance = CalculateDistance(lastUpdate.Latitude, lastUpdate.Longitude,
                                               locationUpdate.Latitude, locationUpdate.Longitude);
                var speed = distance / timeDiff * 3.6; // Convert m/s to km/h

                // Simple speed check - this should be enhanced with vehicle-specific limits
                if (speed > 400) // 400 km/h max
                {
                    locationUpdate.IsSuspicious = true;
                    locationUpdate.SuspiciousReason = $"Speed too high: {speed:F1} km/h";
                    _logger.LogWarning("Suspicious location update: User {UserId} in race {RaceId} - {Reason}", 
                        userId, raceId, locationUpdate.SuspiciousReason);
                }
            }
        }

        await _raceRepository.AppendLocationAsync(locationUpdate);

        // Enqueue for ranking processing
        await EnqueueLocationUpdateAsync(locationUpdate);

        _logger.LogDebug("Location update submitted for user {UserId} in race {RaceId}", userId, raceId);
    }

    public async Task<RaceLiveStatusDto?> GetLiveStatusAsync(Guid raceId)
    {
        var race = await _raceRepository.GetWithRacersAsync(raceId);
        if (race == null)
            return null;

        // Get live rankings from Redis
        var rankings = await _redis.SortedSetRangeByRankWithScoresAsync(
            $"race:{raceId}:positions", 0, -1, Order.Descending);

        var racerStatuses = new List<RacerStatusDto>();
        var rank = 1;

        foreach (var item in rankings)
        {
            if (Guid.TryParse(item.Element, out var userId))
            {
                var racer = race.Racers.FirstOrDefault(r => r.UserId == userId);
                var user = await _userRepository.GetAsync(userId);
                var lastLocation = await _raceRepository.GetLastLocationAsync(raceId, userId);

                if (racer != null && user != null)
                {
                    racerStatuses.Add(new RacerStatusDto
                    {
                        UserId = userId,
                        Username = user.Username,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Rank = rank++,
                        LastLocation = lastLocation != null ? new LocationDto
                        {
                            Latitude = lastLocation.Latitude,
                            Longitude = lastLocation.Longitude,
                            Speed = lastLocation.Speed,
                            Heading = lastLocation.Heading,
                            RecordedAt = lastLocation.RecordedAt
                        } : null,
                        DistanceAlongRoute = Math.Abs(item.Score), // Score is negative distance
                        Finished = racer.Status == "finished",
                        FinishTime = racer.FinishTime
                    });
                }
            }
        }

        return new RaceLiveStatusDto
        {
            RaceId = raceId,
            Status = race.Status,
            Racers = racerStatuses,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<RaceLiveStatusDto?> GetResultsAsync(Guid raceId)
    {
        var race = await _raceRepository.GetWithRacersAsync(raceId);
        if (race == null || race.Status != "finished")
            return null;

        // Get final results from database (not Redis cache)
        var finishedRacers = race.Racers
            .Where(r => r.Status == "finished" && r.FinalRank.HasValue)
            .OrderBy(r => r.FinalRank)
            .ToList();

        var racerStatuses = new List<RacerStatusDto>();

        foreach (var racer in finishedRacers)
        {
            var user = await _userRepository.GetAsync(racer.UserId);
            if (user != null)
            {
                racerStatuses.Add(new RacerStatusDto
                {
                    UserId = racer.UserId,
                    Username = user.Username,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Rank = racer.FinalRank ?? 0,
                    Finished = true,
                    FinishTime = racer.FinishTime,
                    DistanceAlongRoute = racer.FinalDistance
                });
            }
        }

        return new RaceLiveStatusDto
        {
            RaceId = raceId,
            Status = race.Status,
            Racers = racerStatuses,
            LastUpdated = race.FinishedAt ?? DateTime.UtcNow
        };
    }

    public async Task FinishRaceAsync(Guid raceId)
    {
        var race = await _raceRepository.GetWithRacersAsync(raceId);
        if (race == null)
            throw new ArgumentException("Race not found");

        race.Status = "finished";
        race.FinishedAt = DateTime.UtcNow;

        // Save final rankings from Redis to database
        var rankings = await _redis.SortedSetRangeByRankWithScoresAsync(
            $"race:{raceId}:positions", 0, -1, Order.Descending);

        var rank = 1;
        foreach (var item in rankings)
        {
            if (Guid.TryParse(item.Element, out var userId))
            {
                var racer = race.Racers.FirstOrDefault(r => r.UserId == userId);
                if (racer != null)
                {
                    racer.Status = "finished";
                    racer.FinalRank = rank++;
                    racer.FinalDistance = Math.Abs(item.Score);
                }
            }
        }

        await _raceRepository.UpdateAsync(race);

        // Clean up Redis data
        await _redis.KeyDeleteAsync($"race:{raceId}:positions");

        _logger.LogInformation("Race {RaceId} finished", raceId);

        // Broadcast final results
        var results = await GetResultsAsync(raceId);
        if (results != null)
        {
            await _raceHubContext.Clients.Group($"race:{raceId}")
                .SendAsync("RaceFinished", results);
        }
    }

    public async Task BroadcastRaceUpdateAsync(Guid raceId, RaceLiveStatusDto status)
    {
        await _raceHubContext.Clients.Group($"race:{raceId}")
            .SendAsync("RaceUpdated", status);
    }

    private async Task InitializeRaceRankingAsync(Guid raceId)
    {
        // Initialize all racers with score 0
        var race = await _raceRepository.GetWithRacersAsync(raceId);
        if (race?.Racers != null)
        {
            var tasks = race.Racers
                .Where(r => r.Status == "racing")
                .Select(r => _redis.SortedSetAddAsync($"race:{raceId}:positions", r.UserId.ToString(), 0));
            
            await Task.WhenAll(tasks);
        }
    }

    private async Task EnqueueLocationUpdateAsync(RacerLocationUpdate update)
    {
        // Add to Redis Stream for processing by ranking worker
        var values = new NameValueEntry[]
        {
            new("raceId", update.RaceId.ToString()),
            new("userId", update.RacerUserId.ToString()),
            new("latitude", update.Latitude.ToString()),
            new("longitude", update.Longitude.ToString()),
            new("speed", update.Speed?.ToString() ?? ""),
            new("heading", update.Heading?.ToString() ?? ""),
            new("recordedAt", update.RecordedAt.ToString("O")),
            new("isSuspicious", update.IsSuspicious.ToString())
        };

        await _redis.StreamAddAsync("racer_location_updates", values);
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth's radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private async Task<RaceDto> MapRaceToDto(Race race)
    {
        var racers = race.Racers?.Select(async r =>
        {
            var user = await _userRepository.GetAsync(r.UserId);
            return new RaceRacerDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Username = user?.Username ?? "",
                ProfilePictureUrl = user?.ProfilePictureUrl,
                StartOrder = r.StartOrder,
                VehicleId = r.VehicleId,
                Status = r.Status,
                JoinedAt = r.JoinedAt,
                FinishTime = r.FinishTime,
                FinalRank = r.FinalRank
            };
        }).ToArray();

        return new RaceDto
        {
            Id = race.Id,
            HostId = race.HostId,
            Name = race.Name,
            StartLatitude = race.StartPoint.Y,
            StartLongitude = race.StartPoint.X,
            EndLatitude = race.EndPoint.Y,
            EndLongitude = race.EndPoint.X,
            Status = race.Status,
            ScheduledAt = race.ScheduledAt,
            StartedAt = race.StartedAt,
            FinishedAt = race.FinishedAt,
            CreatedAt = race.CreatedAt,
            Racers = racers != null ? await Task.WhenAll(racers) : new List<RaceRacerDto>()
        };
    }
}