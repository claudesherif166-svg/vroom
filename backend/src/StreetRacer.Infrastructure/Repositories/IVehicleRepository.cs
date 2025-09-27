using StreetRacer.Application.Common;
using StreetRacer.Domain.Entities;

namespace StreetRacer.Infrastructure.Repositories;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<PagedResult<Vehicle>> GetByOwnerAsync(Guid? ownerId, Cursor cursor);
}

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetWithContributorsAsync(Guid eventId);
    Task AddContributorAsync(EventContributor contributor);
    Task RemoveContributorAsync(Guid contributorId);
}