using Microsoft.EntityFrameworkCore;
using StreetRacer.Application.Common;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Data;

namespace StreetRacer.Infrastructure.Repositories;

public class EfRaceRepository : EfRepository<Race>, IRaceRepository
{
    public EfRaceRepository(StreetRacerDbContext context) : base(context)
    {
    }

    public async Task<Race?> GetWithRacersAsync(Guid raceId)
    {
        return await _context.Races
            .Include(r => r.Racers)
            .ThenInclude(rr => rr.User)
            .Include(r => r.Racers)
            .ThenInclude(rr => rr.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == raceId);
    }

    public async Task AddRacerAsync(RaceRacer racer)
    {
        await _context.RaceRacers.AddAsync(racer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRacerAsync(RaceRacer racer)
    {
        _context.RaceRacers.Update(racer);
        await _context.SaveChangesAsync();
    }

    public async Task<RaceRacer?> GetRaceRacerAsync(Guid raceId, Guid userId)
    {
        return await _context.RaceRacers
            .FirstOrDefaultAsync(rr => rr.RaceId == raceId && rr.UserId == userId);
    }

    public async Task<ICollection<RaceRacer>> GetRaceRacersAsync(Guid raceId)
    {
        return await _context.RaceRacers
            .Where(rr => rr.RaceId == raceId)
            .ToListAsync();
    }

    public async Task AppendLocationAsync(RacerLocationUpdate update)
    {
        await _context.RacerLocationUpdates.AddAsync(update);
        await _context.SaveChangesAsync();
    }

    public async Task<RacerLocationUpdate?> GetLastLocationAsync(Guid raceId, Guid userId)
    {
        return await _context.RacerLocationUpdates
            .Where(rlu => rlu.RaceId == raceId && rlu.RacerUserId == userId)
            .OrderByDescending(rlu => rlu.RecordedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RacerLocationUpdate>> GetRecentLocationsAsync(Guid raceId, Guid userId, DateTime since)
    {
        return await _context.RacerLocationUpdates
            .Where(rlu => rlu.RaceId == raceId && rlu.RacerUserId == userId && rlu.RecordedAt > since)
            .OrderByDescending(rlu => rlu.RecordedAt)
            .Take(100) // Limit to prevent excessive data
            .ToListAsync();
    }
}