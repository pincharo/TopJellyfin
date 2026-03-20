using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TopJellyfin.Models;
using TopJellyfin.Services;

namespace TopJellyfin.Api;

[ApiController]
[Route("TopJellyfin")]
public class TopJellyfinController : ControllerBase
{
    private readonly RecentlyReleasedService _recentlyReleasedService;
    private readonly TraktService _traktService;
    private readonly LibraryMatchingService _libraryMatchingService;
    private readonly ILogger<TopJellyfinController> _logger;

    public TopJellyfinController(
        RecentlyReleasedService recentlyReleasedService,
        TraktService traktService,
        LibraryMatchingService libraryMatchingService,
        ILogger<TopJellyfinController> logger)
    {
        _recentlyReleasedService = recentlyReleasedService;
        _traktService = traktService;
        _libraryMatchingService = libraryMatchingService;
        _logger = logger;
    }

    [HttpGet("RecentMovies")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<SectionItemDto>> GetRecentMovies()
    {
        var config = Plugin.Instance?.Configuration;
        if (config is { EnableRecentMovies: false })
        {
            return Ok(new List<SectionItemDto>());
        }

        var items = _recentlyReleasedService.GetRecentMovies();
        return Ok(items);
    }

    [HttpGet("RecentSeries")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<SectionItemDto>> GetRecentSeries()
    {
        var config = Plugin.Instance?.Configuration;
        if (config is { EnableRecentSeries: false })
        {
            return Ok(new List<SectionItemDto>());
        }

        var items = _recentlyReleasedService.GetRecentSeries();
        return Ok(items);
    }

    [HttpGet("TopMovies")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SectionItemDto>>> GetTopMovies()
    {
        var config = Plugin.Instance?.Configuration;
        if (config is { EnableTopMovies: false })
        {
            return Ok(new List<SectionItemDto>());
        }

        var traktItems = await _traktService.GetTrendingMoviesAsync().ConfigureAwait(false);
        var matched = _libraryMatchingService.MatchMovies(traktItems);
        return Ok(matched);
    }

    [HttpGet("TopSeries")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SectionItemDto>>> GetTopSeries()
    {
        var config = Plugin.Instance?.Configuration;
        if (config is { EnableTopSeries: false })
        {
            return Ok(new List<SectionItemDto>());
        }

        var traktItems = await _traktService.GetTrendingShowsAsync().ConfigureAwait(false);
        var matched = _libraryMatchingService.MatchShows(traktItems);
        return Ok(matched);
    }

    [HttpGet("ClientScript")]
    [AllowAnonymous]
    [Produces("application/javascript")]
    public ActionResult GetClientScript()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("TopJellyfin.ClientScript.topjellyfin.js");
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, "application/javascript");
    }
}
