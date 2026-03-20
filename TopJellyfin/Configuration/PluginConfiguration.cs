using MediaBrowser.Model.Plugins;

namespace TopJellyfin.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public string TraktClientId { get; set; } = string.Empty;

    public int CacheDurationMinutes { get; set; } = 60;

    public int RecentlyReleasedDays { get; set; } = 90;

    public int MaxItemsPerSection { get; set; } = 20;

    public bool EnableRecentMovies { get; set; } = true;

    public bool EnableRecentSeries { get; set; } = true;

    public bool EnableTopMovies { get; set; } = true;

    public bool EnableTopSeries { get; set; } = true;

    /// <summary>
    /// "trending" or "popular".
    /// </summary>
    public string TraktMoviesEndpoint { get; set; } = "trending";

    /// <summary>
    /// "trending" or "popular".
    /// </summary>
    public string TraktShowsEndpoint { get; set; } = "trending";
}
