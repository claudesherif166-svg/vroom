using Microsoft.Extensions.Logging;
using StreetRacer.Application.Common;
using StreetRacer.Application.Services;

namespace StreetRacer.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SearchService> _logger;

    public SearchService(IUserRepository userRepository, ILogger<SearchService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<PagedResult<object>> SearchAsync(string type, string query, Cursor cursor)
    {
        // Simple implementation - in production, use Elasticsearch
        switch (type.ToLower())
        {
            case "users":
                var users = await _userRepository.SearchByUsernameAsync(query, cursor);
                return new PagedResult<object>
                {
                    Items = users.Items.Cast<object>(),
                    NextCursor = users.NextCursor,
                    HasMore = users.HasMore,
                    TotalCount = users.TotalCount
                };
            
            default:
                return new PagedResult<object>
                {
                    Items = new List<object>(),
                    NextCursor = null,
                    HasMore = false,
                    TotalCount = 0
                };
        }
    }
}