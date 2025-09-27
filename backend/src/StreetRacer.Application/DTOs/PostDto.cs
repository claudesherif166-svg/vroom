using System.ComponentModel.DataAnnotations;

namespace StreetRacer.Application.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string? AuthorProfilePictureUrl { get; set; }
    public string? Text { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<PostMediaDto> Media { get; set; } = new List<PostMediaDto>();
}

public class CreatePostDto
{
    [MaxLength(2000)]
    public string? Text { get; set; }
    
    [MaxLength(50)]
    public string Visibility { get; set; } = "public";
    
    public ICollection<CreatePostMediaDto> Media { get; set; } = new List<CreatePostMediaDto>();
}

public class PostMediaDto
{
    public Guid Id { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class CreatePostMediaDto
{
    [Required]
    public string MediaUrl { get; set; } = string.Empty;
    
    [Required]
    public string MediaType { get; set; } = string.Empty;
    
    public int OrderIndex { get; set; }
}