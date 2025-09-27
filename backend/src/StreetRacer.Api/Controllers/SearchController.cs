using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreetRacer.Application.Common;
using StreetRacer.Application.Services;

namespace StreetRacer.Api.Controllers;

[ApiController]
[Route("api/v1/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<object>>> Search(
        [FromQuery] string type,
        [FromQuery] string q,
        [FromQuery] string? cursor = null)
    {
        var cursorObj = Cursor.FromBase64(cursor);
        var results = await _searchService.SearchAsync(type, q, cursorObj);
        return Ok(results);
    }
}