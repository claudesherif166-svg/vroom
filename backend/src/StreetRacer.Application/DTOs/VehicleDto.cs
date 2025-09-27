using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace StreetRacer.Application.DTOs;

public class VehicleDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string? Name { get; set; }
    public string Type { get; set; } = string.Empty;
    public JsonDocument? DesignMeta { get; set; }
    public string? Model3DUrl { get; set; }
    public double MaxSpeed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVehicleDto
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "car";
    
    public JsonDocument? DesignMeta { get; set; }
    
    [MaxLength(500)]
    public string? Model3DUrl { get; set; }
    
    public double MaxSpeed { get; set; } = 200.0;
}

public class UpdateVehicleDto
{
    [MaxLength(255)]
    public string? Name { get; set; }
    
    public JsonDocument? DesignMeta { get; set; }
    
    [MaxLength(500)]
    public string? Model3DUrl { get; set; }
}

public class UserAchievementDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public DateTime AwardedAt { get; set; }
    public JsonDocument? Progress { get; set; }
}