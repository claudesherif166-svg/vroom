using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Common;

public interface IDataModel
{
    Guid Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    DateTime? DeletedAt { get; set; }
}

public abstract class DataModelBase : IDataModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public bool IsDeleted => DeletedAt.HasValue;
}