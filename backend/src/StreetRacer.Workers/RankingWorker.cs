using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StreetRacer.Application.Services;
using StreetRacer.Infrastructure.Repositories;
using System.Text.Json;

namespace StreetRacer.Workers;

public class RankingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RankingWorker> _logger;
    private readonly IDatabase _database;

    public RankingWorker(
        IServiceProvider serviceProvider,
        IConnectionMultiplexer redis,
        ILogger<RankingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _redis = redis;
        _logger = logger;
        _database = redis.GetDatabase();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ranking Worker started");

        await Task.Delay(5000, stoppingToken); // Wait for services to initialize

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Process location updates from Redis Stream
                await ProcessLocationUpdates(stoppingToken);
                
                // Broadcast live rankings every second
                await BroadcastLiveRankings(stoppingToken);
                
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ranking worker");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessLocationUpdates(CancellationToken cancellationToken)
    {
        try
        {
            // Read from Redis Stream
            var results = await _database.StreamReadAsync("racer_location_updates", "$", 100);
            
            if (!results.Any())
                return;

            using var scope = _serviceProvider.CreateScope();
            var raceRepository = scope.ServiceProvider.GetRequiredService<IRaceRepository>();

            foreach (var streamEntry in results)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await ProcessLocationUpdate(streamEntry, raceRepository);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing location update {StreamId}", streamEntry.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from location updates stream");
        }
    }

    private async Task ProcessLocationUpdate(StreamEntry entry, IRaceRepository raceRepository)
    {
        var values = entry.Values.ToDictionary(v => v.Name.ToString(), v => v.Value.ToString());

        if (!Guid.TryParse(values.GetValueOrDefault("raceId"), out var raceId) ||
            !Guid.TryParse(values.GetValueOrDefault("userId"), out var userId))
        {
            _logger.LogWarning("Invalid race or user ID in location update");
            return;
        }

        if (!double.TryParse(values.GetValueOrDefault("latitude"), out var lat) ||
            !double.TryParse(values.GetValueOrDefault("longitude"), out var lng))
        {
            _logger.LogWarning("Invalid coordinates in location update");
            return;
        }

        var isSuspicious = bool.Parse(values.GetValueOrDefault("isSuspicious", "false"));
        if (isSuspicious)
        {
            _logger.LogWarning("Skipping suspicious location update for user {UserId} in race {RaceId}", userId, raceId);
            return;
        }

        // Get race information
        var race = await raceRepository.GetAsync(raceId);
        if (race == null || race.Status != "in_progress")
        {
            return;
        }

        // Calculate distance along route or distance to finish
        double distanceScore = 0;

        if (race.Route != null)
        {
            // Use PostGIS to calculate distance along route
            // This is a simplified version - in production, you'd use proper PostGIS queries
            distanceScore = CalculateDistanceAlongRoute(lat, lng, race);
        }
        else
        {
            // Calculate distance to finish point
            distanceScore = CalculateDistanceToFinish(lat, lng, race.EndPoint.Y, race.EndPoint.X);
        }

        // Update Redis sorted set (negative score so closest to finish is rank 1)
        await _database.SortedSetAddAsync($"race:{raceId}:positions", userId.ToString(), -distanceScore);

        // Check if racer finished (within 50 meters of finish)
        if (distanceScore < 50)
        {
            await HandleRacerFinished(raceId, userId, raceRepository);
        }

        _logger.LogDebug("Updated ranking for user {UserId} in race {RaceId}: distance = {Distance}m", 
            userId, raceId, distanceScore);
    }

    private double CalculateDistanceAlongRoute(double lat, double lng, Domain.Entities.Race race)
    {
        // Simplified calculation - in production, use PostGIS ST_LineLocatePoint
        // For now, calculate distance from start point as a proxy
        return CalculateDistance(race.StartPoint.Y, race.StartPoint.X, lat, lng);
    }

    private double CalculateDistanceToFinish(double lat, double lng, double finishLat, double finishLng)
    {
        return CalculateDistance(lat, lng, finishLat, finishLng);
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

    private async Task HandleRacerFinished(Guid raceId, Guid userId, IRaceRepository raceRepository)
    {
        var racer = await raceRepository.GetRaceRacerAsync(raceId, userId);
        if (racer?.Status == "racing")
        {
            racer.Status = "finished";
            racer.FinishTime = TimeSpan.FromSeconds((DateTime.UtcNow - racer.Race.StartedAt!.Value).TotalSeconds);
            
            await raceRepository.UpdateRacerAsync(racer);
            
            _logger.LogInformation("Racer {UserId} finished race {RaceId}", userId, raceId);
        }
    }

    private async Task BroadcastLiveRankings(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var raceService = scope.ServiceProvider.GetRequiredService<IRaceService>();
            
            // Get all active races from Redis keys
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: "race:*:positions").ToList();

            foreach (var key in keys)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var keyStr = key.ToString();
                var raceIdStr = keyStr.Split(':')[1];
                
                if (Guid.TryParse(raceIdStr, out var raceId))
                {
                    var status = await raceService.GetLiveStatusAsync(raceId);
                    if (status != null)
                    {
                        await raceService.BroadcastRaceUpdateAsync(raceId, status);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting live rankings");
        }
    }
}