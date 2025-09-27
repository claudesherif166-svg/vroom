using StreetRacer.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace StreetRacer.Domain.Entities;

public class Achievement : DataModelBase
{
    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    public string? IconUrl { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}

public class UserAchievement : DataModelBase
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid AchievementId { get; set; }
    public virtual Achievement Achievement { get; set; } = null!;
    
    [Required]
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    
    public JsonDocument? Progress { get; set; }
}