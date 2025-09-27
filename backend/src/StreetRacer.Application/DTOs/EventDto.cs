using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Application.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public string HostUsername { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string JoinPolicy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<EventContributorDto> Contributors { get; set; } = new List<EventContributorDto>();
}

public class CreateEventDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    
    [MaxLength(50)]
    public string Visibility { get; set; } = "public";
    
    [MaxLength(50)]
    public string JoinPolicy { get; set; } = "auto";
}

public class UpdateEventDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public double? LocationLatitude { get; set; }
    public double? LocationLongitude { get; set; }
    
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    
    [MaxLength(50)]
    public string Visibility { get; set; } = "public";
    
    [MaxLength(50)]
    public string JoinPolicy { get; set; } = "auto";
}

public class EventContributorDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid GrantedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}