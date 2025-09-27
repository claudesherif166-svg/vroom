using StreetRacer.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class RaceRacer : DataModelBase
{
    public Guid RaceId { get; set; }
    public virtual Race Race { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public int StartOrder { get; set; }
    
    public Guid? VehicleId { get; set; }
    public virtual Vehicle? Vehicle { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; } = "invited"; // invited, accepted, racing, finished
    
    public DateTime? JoinedAt { get; set; }
    
    public TimeSpan? FinishTime { get; set; }
    
    public double? FinalDistance { get; set; }
    
    public int? FinalRank { get; set; }
}