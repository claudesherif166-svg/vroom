using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;

namespace StreetRacer.Application.Services;

public interface IVehicleService
{
    Task<VehicleDto> CreateVehicleAsync(Guid ownerId, CreateVehicleDto createVehicleDto);
    Task<VehicleDto?> GetVehicleAsync(Guid vehicleId);
    Task<VehicleDto> UpdateVehicleAsync(Guid vehicleId, Guid ownerId, UpdateVehicleDto updateVehicleDto);
    Task DeleteVehicleAsync(Guid vehicleId, Guid ownerId);
    Task<PagedResult<VehicleDto>> GetVehiclesAsync(Guid? ownerId, Cursor cursor);
}

public interface IEventService
{
    Task<EventDto> CreateEventAsync(Guid hostId, CreateEventDto createEventDto);
    Task<EventDto?> GetEventAsync(Guid eventId);
    Task<EventDto> UpdateEventAsync(Guid eventId, Guid hostId, UpdateEventDto updateEventDto);
    Task<PagedResult<EventDto>> GetEventsAsync(Cursor cursor);
    Task AddContributorAsync(Guid eventId, Guid hostId, Guid userId, string role);
    Task RemoveContributorAsync(Guid eventId, Guid hostId, Guid contributorId);
}

public interface ISearchService
{
    Task<PagedResult<object>> SearchAsync(string type, string query, Cursor cursor);
}