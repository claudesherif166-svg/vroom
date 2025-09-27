using StreetRacer.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class Story : DataModelBase
{
    public Guid AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;
    
    [Required]
    [MaxLength(500)]
    public string MediaUrl { get; set; } = string.Empty;
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}