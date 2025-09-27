using StreetRacer.Domain.Common;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class Race : DataModelBase
{
    public Guid HostId { get; set; }
    public virtual User Host { get; set; } = null!;
    
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [Required]
    public Point StartPoint { get; set; } = null!;
    
    [Required]
    public Point EndPoint { get; set; } = null!;
    
    public LineString? Route { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "created"; // created, invited, accepted, in_progress, finished, cancelled
    
    public DateTime? ScheduledAt { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? FinishedAt { get; set; }
    
    public Guid? EventId { get; set; }
    public virtual Event? Event { get; set; }
    
    // Navigation properties
    public virtual ICollection<RaceRacer> Racers { get; set; } = new List<RaceRacer>();
    public virtual ICollection<RacerLocationUpdate> LocationUpdates { get; set; } = new List<RacerLocationUpdate>();
}