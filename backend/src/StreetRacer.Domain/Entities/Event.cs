using StreetRacer.Domain.Common;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class Event : DataModelBase
{
    public Guid HostId { get; set; }
    public virtual User Host { get; set; } = null!;
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public Point? Location { get; set; }
    
    public DateTime? StartAt { get; set; }
    
    public DateTime? EndAt { get; set; }
    
    [MaxLength(50)]
    public string Visibility { get; set; } = "public"; // public, private
    
    [MaxLength(50)]
    public string JoinPolicy { get; set; } = "auto"; // auto, request
    
    // Navigation properties
    public virtual ICollection<EventContributor> Contributors { get; set; } = new List<EventContributor>();
    public virtual ICollection<Race> Races { get; set; } = new List<Race>();
}

public class EventContributor : DataModelBase
{
    public Guid EventId { get; set; }
    public virtual Event Event { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    [MaxLength(50)]
    public string Role { get; set; } = "contributor"; // contributor, moderator
    
    public Guid GrantedBy { get; set; }
    public virtual User GrantedByUser { get; set; } = null!;
}