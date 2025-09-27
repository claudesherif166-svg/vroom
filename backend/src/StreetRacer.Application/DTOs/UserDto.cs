using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int RacesCount { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
}

public class UpdateUserDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    public bool IsPrivate { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int RacesCount { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
    public ICollection<VehicleDto> Vehicles { get; set; } = new List<VehicleDto>();
    public ICollection<UserAchievementDto> Achievements { get; set; } = new List<UserAchievementDto>();
}