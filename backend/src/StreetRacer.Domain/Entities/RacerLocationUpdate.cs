using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class RacerLocationUpdate
{
    [Key]
    public long Id { get; set; }
    
    public Guid RaceId { get; set; }
    public virtual Race Race { get; set; } = null!;
    
    public Guid RacerUserId { get; set; }
    public virtual User RacerUser { get; set; } = null!;
    
    [Required]
    public double Latitude { get; set; }
    
    [Required]
    public double Longitude { get; set; }
    
    public double? Speed { get; set; }
    
    public double? Heading { get; set; }
    
    [Required]
    public DateTime RecordedAt { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsSuspicious { get; set; } = false;
    
    public string? SuspiciousReason { get; set; }
}