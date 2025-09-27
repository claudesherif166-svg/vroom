using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Application.DTOs;

public class RaceDto
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string? Name { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<RaceRacerDto> Racers { get; set; } = new List<RaceRacerDto>();
}

public class CreateRaceDto
{
    [Required]
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [Required]
    public ICollection<Guid> RacerUserIds { get; set; } = new List<Guid>();
    
    [Required]
    public double StartLatitude { get; set; }
    
    [Required]
    public double StartLongitude { get; set; }
    
    [Required]
    public double EndLatitude { get; set; }
    
    [Required]
    public double EndLongitude { get; set; }
    
    public object? RouteGeoJson { get; set; }
    
    public DateTime? ScheduledAt { get; set; }
}

public class RaceRacerDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public int StartOrder { get; set; }
    public Guid? VehicleId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? JoinedAt { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinalRank { get; set; }
}

public class LocationDto
{
    [Required]
    public double Latitude { get; set; }
    
    [Required]
    public double Longitude { get; set; }
    
    public double? Speed { get; set; }
    
    public double? Heading { get; set; }
    
    [Required]
    public DateTime RecordedAt { get; set; }
}

public class RaceLiveStatusDto
{
    public Guid RaceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public ICollection<RacerStatusDto> Racers { get; set; } = new List<RacerStatusDto>();
    public DateTime LastUpdated { get; set; }
}

public class RacerStatusDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public int Rank { get; set; }
    public LocationDto? LastLocation { get; set; }
    public double? DistanceAlongRoute { get; set; }
    public bool Finished { get; set; }
    public TimeSpan? FinishTime { get; set; }
}