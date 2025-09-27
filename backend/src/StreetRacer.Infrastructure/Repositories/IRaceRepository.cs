using StreetRacer.Application.Common;
using StreetRacer.Domain.Entities;

namespace StreetRacer.Infrastructure.Repositories;

public interface IRaceRepository : IRepository<Race>
{
    Task<Race?> GetWithRacersAsync(Guid raceId);
    Task AddRacerAsync(RaceRacer racer);
    Task UpdateRacerAsync(RaceRacer racer);
    Task<RaceRacer?> GetRaceRacerAsync(Guid raceId, Guid userId);
    Task<ICollection<RaceRacer>> GetRaceRacersAsync(Guid raceId);
    Task AppendLocationAsync(RacerLocationUpdate update);
    Task<RacerLocationUpdate?> GetLastLocationAsync(Guid raceId, Guid userId);
    Task<IEnumerable<RacerLocationUpdate>> GetRecentLocationsAsync(Guid raceId, Guid userId, DateTime since);
}

public interface IRepository<T> where T : class
{
    Task<T?> GetAsync(Guid id);
    Task<PagedResult<T>> ListAsync(Cursor cursor);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByKeycloakSubjectAsync(string subject);
}