using Microsoft.Extensions.Logging;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;
using StreetRacer.Domain.Entities;
using StreetRacer.Infrastructure.Repositories;

namespace StreetRacer.Infrastructure.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PostService> _logger;

    public PostService(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ILogger<PostService> logger)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<PostDto> CreatePostAsync(Guid authorId, CreatePostDto createPostDto)
    {
        var author = await _userRepository.GetAsync(authorId);
        if (author == null)
            throw new ArgumentException("Author not found");

        var post = new Post
        {
            AuthorId = authorId,
            Text = createPostDto.Text,
            Visibility = createPostDto.Visibility
        };

        await _postRepository.AddAsync(post);

        // Add media if provided
        foreach (var mediaDto in createPostDto.Media)
        {
            var media = new PostMedia
            {
                PostId = post.Id,
                MediaUrl = mediaDto.MediaUrl,
                MediaType = mediaDto.MediaType,
                OrderIndex = mediaDto.OrderIndex
            };
            await _postRepository.AddMediaAsync(media);
        }

        return await MapToPostDto(post, authorId);
    }

    public async Task<PostDto?> GetPostAsync(Guid postId, Guid? currentUserId = null)
    {
        var post = await _postRepository.GetWithMediaAsync(postId);
        if (post == null) return null;

        return await MapToPostDto(post, currentUserId);
    }

    public async Task DeletePostAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found");

        if (post.AuthorId != userId)
            throw new UnauthorizedAccessException("Not authorized to delete this post");

        await _postRepository.DeleteAsync(postId);
    }

    public async Task LikePostAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found");

        var existingLike = await _postRepository.GetLikeAsync(postId, userId);
        if (existingLike != null)
            return; // Already liked

        var like = new PostLike
        {
            PostId = postId,
            UserId = userId
        };

        await _postRepository.AddLikeAsync(like);
        
        // Update likes count
        post.LikesCount++;
        await _postRepository.UpdateAsync(post);
    }

    public async Task UnlikePostAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetAsync(postId);
        if (post == null)
            throw new ArgumentException("Post not found");

        var like = await _postRepository.GetLikeAsync(postId, userId);
        if (like == null)
            return; // Not liked

        await _postRepository.RemoveLikeAsync(like.Id);
        
        // Update likes count
        post.LikesCount = Math.Max(0, post.LikesCount - 1);
        await _postRepository.UpdateAsync(post);
    }

    public async Task<PagedResult<PostDto>> GetUserPostsAsync(Guid userId, Guid? currentUserId, Cursor cursor)
    {
        var posts = await _postRepository.GetByAuthorAsync(userId, cursor);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts.Items)
        {
            postDtos.Add(await MapToPostDto(post, currentUserId));
        }

        return new PagedResult<PostDto>
        {
            Items = postDtos,
            NextCursor = posts.NextCursor,
            HasMore = posts.HasMore,
            TotalCount = posts.TotalCount
        };
    }

    private async Task<PostDto> MapToPostDto(Post post, Guid? currentUserId)
    {
        var author = await _userRepository.GetAsync(post.AuthorId);
        var isLiked = currentUserId.HasValue ? 
            await _postRepository.GetLikeAsync(post.Id, currentUserId.Value) != null : false;

        var media = post.Media?.Select(m => new PostMediaDto
        {
            Id = m.Id,
            MediaUrl = m.MediaUrl,
            MediaType = m.MediaType,
            OrderIndex = m.OrderIndex
        }).ToList() ?? new List<PostMediaDto>();

        return new PostDto
        {
            Id = post.Id,
            AuthorId = post.AuthorId,
            AuthorUsername = author?.Username ?? "",
            AuthorProfilePictureUrl = author?.ProfilePictureUrl,
            Text = post.Text,
            Visibility = post.Visibility,
            LikesCount = post.LikesCount,
            CommentsCount = post.CommentsCount,
            IsLikedByCurrentUser = isLiked,
            CreatedAt = post.CreatedAt,
            Media = media
        };
    }
}