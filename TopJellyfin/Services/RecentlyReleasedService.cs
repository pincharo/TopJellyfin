using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using TopJellyfin.Configuration;
using TopJellyfin.Models;

namespace TopJellyfin.Services;

public class RecentlyReleasedService
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<RecentlyReleasedService> _logger;

    public RecentlyReleasedService(ILibraryManager libraryManager, ILogger<RecentlyReleasedService> logger)
    {
        _libraryManager = libraryManager;
        _logger = logger;
    }

    public List<SectionItemDto> GetRecentMovies()
    {
        return GetRecentItems(BaseItemKind.Movie);
    }

    public List<SectionItemDto> GetRecentSeries()
    {
        return GetRecentItems(BaseItemKind.Series);
    }

    private List<SectionItemDto> GetRecentItems(BaseItemKind itemKind)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

        var query = new InternalItemsQuery
        {
            IncludeItemTypes = new[] { itemKind },
            MinPremiereDate = DateTime.UtcNow.AddDays(-config.RecentlyReleasedDays),
            OrderBy = new[] { (ItemSortBy.PremiereDate, SortOrder.Descending) },
            Limit = config.MaxItemsPerSection,
            IsVirtualItem = false,
            Recursive = true
        };

        try
        {
            var items = _libraryManager.GetItemList(query);
            return items.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying recently released {ItemKind}", itemKind);
            return new List<SectionItemDto>();
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
