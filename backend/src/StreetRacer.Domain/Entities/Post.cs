using StreetRacer.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Domain.Entities;

public class Post : DataModelBase
{
    public Guid AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;
    
    [MaxLength(2000)]
    public string? Text { get; set; }
    
    [MaxLength(50)]
    public string Visibility { get; set; } = "public"; // public, private, followers
    
    public int LikesCount { get; set; } = 0;
    
    public int CommentsCount { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
    public virtual ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
}

public class PostMedia : DataModelBase
{
    public Guid PostId { get; set; }
    public virtual Post Post { get; set; } = null!;
    
    [Required]
    [MaxLength(500)]
    public string MediaUrl { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string MediaType { get; set; } = "image"; // image, video
    
    public int OrderIndex { get; set; }
}

public class PostLike : DataModelBase
{
    public Guid PostId { get; set; }
    public virtual Post Post { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
}

public class PostComment : DataModelBase
{
    public Guid PostId { get; set; }
    public virtual Post Post { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
}