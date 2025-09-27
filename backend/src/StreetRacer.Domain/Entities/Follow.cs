using StreetRacer.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class Follow : DataModelBase
{
    public Guid FollowerId { get; set; }
    public virtual User Follower { get; set; } = null!;
    
    public Guid FolloweeId { get; set; }
    public virtual User Followee { get; set; } = null!;
    
    [MaxLength(50)]
    public string Status { get; set; } = "accepted"; // requested, accepted, blocked
}