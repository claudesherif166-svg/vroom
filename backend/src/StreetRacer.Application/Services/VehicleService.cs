using Microsoft.Extensions.Logging;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Repositories;

namespace StreetRacer.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        IUserRepository userRepository,
        ILogger<VehicleService> logger)
    {
        _vehicleRepository = vehicleRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<VehicleDto> CreateVehicleAsync(Guid ownerId, CreateVehicleDto createVehicleDto)
    {
        var owner = await _userRepository.GetAsync(ownerId);
        if (owner == null)
            throw new ArgumentException("Owner not found");

        var vehicle = new Vehicle
        {
            OwnerId = ownerId,
            Name = createVehicleDto.Name,
            Type = createVehicleDto.Type,
            DesignMeta = createVehicleDto.DesignMeta,
            Model3DUrl = createVehicleDto.Model3DUrl,
            MaxSpeed = createVehicleDto.MaxSpeed
        };

        await _vehicleRepository.AddAsync(vehicle);

        return MapToVehicleDto(vehicle);
    }

    public async Task<VehicleDto?> GetVehicleAsync(Guid vehicleId)
    {
        var vehicle = await _vehicleRepository.GetAsync(vehicleId);
        if (vehicle == null) return null;

        return MapToVehicleDto(vehicle);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(Guid vehicleId, Guid ownerId, UpdateVehicleDto updateVehicleDto)
    {
        var vehicle = await _vehicleRepository.GetAsync(vehicleId);
        if (vehicle == null)
            throw new ArgumentException("Vehicle not found");

        if (vehicle.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not authorized to update this vehicle");

        vehicle.Name = updateVehicleDto.Name;
        vehicle.DesignMeta = updateVehicleDto.DesignMeta;
        vehicle.Model3DUrl = updateVehicleDto.Model3DUrl;

        await _vehicleRepository.UpdateAsync(vehicle);

        return MapToVehicleDto(vehicle);
    }

    public async Task DeleteVehicleAsync(Guid vehicleId, Guid ownerId)
    {
        var vehicle = await _vehicleRepository.GetAsync(vehicleId);
        if (vehicle == null)
            throw new ArgumentException("Vehicle not found");

        if (vehicle.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not authorized to delete this vehicle");

        await _vehicleRepository.DeleteAsync(vehicleId);
    }

    public async Task<PagedResult<VehicleDto>> GetVehiclesAsync(Guid? ownerId, Cursor cursor)
    {
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerId, cursor);
        
        return new PagedResult<VehicleDto>
        {
            Items = vehicles.Items.Select(MapToVehicleDto),
            NextCursor = vehicles.NextCursor,
            HasMore = vehicles.HasMore,
            TotalCount = vehicles.TotalCount
        };
    }

    private static VehicleDto MapToVehicleDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            OwnerId = vehicle.OwnerId,
            Name = vehicle.Name,
            Type = vehicle.Type,
            DesignMeta = vehicle.DesignMeta,
            Model3DUrl = vehicle.Model3DUrl,
            MaxSpeed = vehicle.MaxSpeed,
            CreatedAt = vehicle.CreatedAt
        };
    }
}