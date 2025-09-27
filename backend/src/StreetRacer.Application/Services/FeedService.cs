using Microsoft.Extensions.Logging;
using StreetRacer.Application.Common;
using StreetRacer.Application.DTOs;
using StreetRacer.Application.Services;

namespace StreetRacer.Infrastructure.Services;

public class FeedService : IFeedService
{
    private readonly IPostRepository _postRepository;
    private readonly IPostService _postService;
    private readonly ILogger<FeedService> _logger;

    public FeedService(
        IPostRepository postRepository,
        IPostService postService,
        ILogger<FeedService> logger)
    {
        _postRepository = postRepository;
        _postService = postService;
        _logger = logger;
    }

    public async Task<PagedResult<PostDto>> GetPersonalizedFeedAsync(Guid userId, Cursor cursor)
    {
        // Simple implementation - get all public posts
        // In production, this would consider following relationships and ranking algorithms
        var posts = await _postRepository.GetPublicPostsAsync(cursor);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts.Items)
        {
            var postDto = await _postService.GetPostAsync(post.Id, userId);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return new PagedResult<PostDto>
        {
            Items = postDtos,
            NextCursor = posts.NextCursor,
            HasMore = posts.HasMore,
            TotalCount = posts.TotalCount
        };
    }

    public async Task<PagedResult<PostDto>> GetPublicFeedAsync(Cursor cursor)
    {
        var posts = await _postRepository.GetPublicPostsAsync(cursor);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts.Items)
        {
            var postDto = await _postService.GetPostAsync(post.Id);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return new PagedResult<PostDto>
        {
            Items = postDtos,
            NextCursor = posts.NextCursor,
            HasMore = posts.HasMore,
            TotalCount = posts.TotalCount
        };
    }
}