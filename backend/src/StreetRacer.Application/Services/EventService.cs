using Microsoft.Extensions.Logging;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Repositories;
using NetTopologySuite.Geometries;
using NetTopologySuite;

namespace StreetRacer.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<EventService> _logger;
    private readonly GeometryFactory _geometryFactory;

    public EventService(
        IEventRepository eventRepository,
        IUserRepository userRepository,
        ILogger<EventService> logger)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _logger = logger;
        
        var geometryServices = NtsGeometryServices.Instance;
        _geometryFactory = geometryServices.CreateGeometryFactory(srid: 4326);
    }

    public async Task<EventDto> CreateEventAsync(Guid hostId, CreateEventDto createEventDto)
    {
        var host = await _userRepository.GetAsync(hostId);
        if (host == null)
            throw new ArgumentException("Host not found");

        Point? location = null;
        if (createEventDto.LocationLatitude.HasValue && createEventDto.LocationLongitude.HasValue)
        {
            location = _geometryFactory.CreatePoint(new Coordinate(
                createEventDto.LocationLongitude.Value, 
                createEventDto.LocationLatitude.Value));
        }

        var eventEntity = new Event
        {
            HostId = hostId,
            Title = createEventDto.Title,
            Description = createEventDto.Description,
            Location = location,
            StartAt = createEventDto.StartAt,
            EndAt = createEventDto.EndAt,
            Visibility = createEventDto.Visibility,
            JoinPolicy = createEventDto.JoinPolicy
        };

        await _eventRepository.AddAsync(eventEntity);

        return await MapToEventDto(eventEntity);
    }

    public async Task<EventDto?> GetEventAsync(Guid eventId)
    {
        var eventEntity = await _eventRepository.GetWithContributorsAsync(eventId);
        if (eventEntity == null) return null;

        return await MapToEventDto(eventEntity);
    }

    public async Task<EventDto> UpdateEventAsync(Guid eventId, Guid hostId, UpdateEventDto updateEventDto)
    {
        var eventEntity = await _eventRepository.GetAsync(eventId);
        if (eventEntity == null)
            throw new ArgumentException("Event not found");

        if (eventEntity.HostId != hostId)
            throw new UnauthorizedAccessException("Not authorized to update this event");

        Point? location = null;
        if (updateEventDto.LocationLatitude.HasValue && updateEventDto.LocationLongitude.HasValue)
        {
            location = _geometryFactory.CreatePoint(new Coordinate(
                updateEventDto.LocationLongitude.Value, 
                updateEventDto.LocationLatitude.Value));
        }

        eventEntity.Title = updateEventDto.Title;
        eventEntity.Description = updateEventDto.Description;
        eventEntity.Location = location;
        eventEntity.StartAt = updateEventDto.StartAt;
        eventEntity.EndAt = updateEventDto.EndAt;
        eventEntity.Visibility = updateEventDto.Visibility;
        eventEntity.JoinPolicy = updateEventDto.JoinPolicy;

        await _eventRepository.UpdateAsync(eventEntity);

        return await MapToEventDto(eventEntity);
    }

    public async Task<PagedResult<EventDto>> GetEventsAsync(Cursor cursor)
    {
        var events = await _eventRepository.ListAsync(cursor);
        
        var eventDtos = new List<EventDto>();
        foreach (var eventEntity in events.Items)
        {
            eventDtos.Add(await MapToEventDto(eventEntity));
        }

        return new PagedResult<EventDto>
        {
            Items = eventDtos,
            NextCursor = events.NextCursor,
            HasMore = events.HasMore,
            TotalCount = events.TotalCount
        };
    }

    public async Task AddContributorAsync(Guid eventId, Guid hostId, Guid userId, string role)
    {
        var eventEntity = await _eventRepository.GetAsync(eventId);
        if (eventEntity == null)
            throw new ArgumentException("Event not found");

        if (eventEntity.HostId != hostId)
            throw new UnauthorizedAccessException("Not authorized to add contributors");

        var contributor = new EventContributor
        {
            EventId = eventId,
            UserId = userId,
            Role = role,
            GrantedBy = hostId
        };

        await _eventRepository.AddContributorAsync(contributor);
    }

    public async Task RemoveContributorAsync(Guid eventId, Guid hostId, Guid contributorId)
    {
        var eventEntity = await _eventRepository.GetAsync(eventId);
        if (eventEntity == null)
            throw new ArgumentException("Event not found");

        if (eventEntity.HostId != hostId)
            throw new UnauthorizedAccessException("Not authorized to remove contributors");

        await _eventRepository.RemoveContributorAsync(contributorId);
    }

    private async Task<EventDto> MapToEventDto(Event eventEntity)
    {
        var host = await _userRepository.GetAsync(eventEntity.HostId);
        
        var contributors = new List<EventContributorDto>();
        if (eventEntity.Contributors != null)
        {
            foreach (var contributor in eventEntity.Contributors)
            {
                var user = await _userRepository.GetAsync(contributor.UserId);
                contributors.Add(new EventContributorDto
                {
                    Id = contributor.Id,
                    UserId = contributor.UserId,
                    Username = user?.Username ?? "",
                    ProfilePictureUrl = user?.ProfilePictureUrl,
                    Role = contributor.Role,
                    GrantedBy = contributor.GrantedBy,
                    CreatedAt = contributor.CreatedAt
                });
            }
        }

        return new EventDto
        {
            Id = eventEntity.Id,
            HostId = eventEntity.HostId,
            HostUsername = host?.Username ?? "",
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            LocationLatitude = eventEntity.Location?.Y,
            LocationLongitude = eventEntity.Location?.X,
            StartAt = eventEntity.StartAt,
            EndAt = eventEntity.EndAt,
            Visibility = eventEntity.Visibility,
            JoinPolicy = eventEntity.JoinPolicy,
            CreatedAt = eventEntity.CreatedAt,
            Contributors = contributors
        };
    }
}