using StreetRacer.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class User : DataModelBase
{
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public bool IsPrivate { get; set; } = false;
    
    [MaxLength(255)]
    public string? KeycloakSubject { get; set; }
    
    // Navigation properties
    public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<Race> HostedRaces { get; set; } = new List<Race>();
    public virtual ICollection<RaceRacer> RaceParticipations { get; set; } = new List<RaceRacer>();
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    public virtual ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
}