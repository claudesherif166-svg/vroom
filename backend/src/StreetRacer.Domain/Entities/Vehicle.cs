using StreetRacer.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace StreetRacer.Domain.Entities;

public class Vehicle : DataModelBase
{
    public Guid OwnerId { get; set; }
    public virtual User Owner { get; set; } = null!;
    
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [MaxLength(50)]
    public string Type { get; set; } = "car"; // car, bike, truck
    
    public JsonDocument? DesignMeta { get; set; }
    
    [MaxLength(500)]
    public string? Model3DUrl { get; set; }
    
    public double MaxSpeed { get; set; } = 200.0; // km/h for anti-cheat
    
    // Navigation properties
    public virtual ICollection<RaceRacer> RaceParticipations { get; set; } = new List<RaceRacer>();
}