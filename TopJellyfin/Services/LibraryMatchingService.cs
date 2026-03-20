using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using TopJellyfin.Configuration;
using TopJellyfin.Models;

namespace TopJellyfin.Services;

public class LibraryMatchingService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<LibraryMatchingService> _logger;

    public LibraryMatchingService(ILibraryManager libraryManager, ILogger<LibraryMatchingService> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public List<SectionItemDto> MatchMovies(List<TraktMovieResult> traktItems)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var results = new List<SectionItemDto>();

        foreach (var traktItem in traktItems)
        {
            if (results.Count >= config.MaxItemsPerSection)
            {
                break;
            }

            var movie = traktItem.Movie;
            if (movie?.Ids == null)
            {
                continue;
            }

            var matched = FindInLibrary(movie.Ids, BaseItemKind.Movie);
            if (matched != null)
            {
                results.Add(matched);
            }
        }

        return results;
    }

    public List<SectionItemDto> MatchShows(List<TraktShowResult> traktItems)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var results = new List<SectionItemDto>();

        foreach (var traktItem in traktItems)
        {
            if (results.Count >= config.MaxItemsPerSection)
            {
                break;
            }

            var show = traktItem.Show;
            if (show?.Ids == null)
            {
                continue;
            }

            var matched = FindInLibrary(show.Ids, BaseItemKind.Series);
            if (matched != null)
            {
                results.Add(matched);
            }
        }

        return results;
    }

    private SectionItemDto? FindInLibrary(TraktIds ids, BaseItemKind itemKind)
    {
        BaseItem? item = null;

        // Try IMDB ID first (most reliable)
        if (!string.IsNullOrEmpty(ids.Imdb))
        {
            item = FindByProviderId("Imdb", ids.Imdb, itemKind);
        }

        // Fallback to TMDB ID
        if (item == null && ids.Tmdb.HasValue && ids.Tmdb.Value > 0)
        {
            item = FindByProviderId("Tmdb", ids.Tmdb.Value.ToString(), itemKind);
        }

        if (item == null)
        {
            return null;
        }

        return MapToDto(item);
    }

    private BaseItem? FindByProviderId(string providerName, string providerId, BaseItemKind itemKind)
    {
        try
        {
            var query = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { itemKind },
                HasAnyProviderId = new Dictionary<string, string> { { providerName, providerId } },
                Recursive = true,
                Limit = 1
            };

            var items = _libraryManager.GetItemList(query);
            return items.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error searching for {Provider}={Id}", providerName, providerId);
            return null;
        }
    }

    private static SectionItemDto MapToDto(BaseItem item)
    {
        return new SectionItemDto
        {
            Id = item.Id,
            Name = item.Name,
            Type = item.GetBaseItemKind().ToString(),
            ProductionYear = item.ProductionYear,
            PremiereDate = item.PremiereDate,
            CommunityRating = item.CommunityRating,
            HasPrimaryImage = item.GetImageInfo(ImageType.Primary, 0) != null,
            HasBackdropImage = item.GetImageInfo(ImageType.Backdrop, 0) != null
        };
    }
}
