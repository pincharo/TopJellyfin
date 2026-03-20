using System.Text.Json.Serialization;

namespace TopJellyfin.Models;

public class TraktMovieResult
{
    [JsonPropertyName("watchers")]
    public int Watchers { get; set; }

    [JsonPropertyName("movie")]
    public TraktMovie? Movie { get; set; }
}

public class TraktMovie
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("ids")]
    public TraktIds? Ids { get; set; }
}

public class TraktIds
{
    [JsonPropertyName("trakt")]
    public int Trakt { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("imdb")]
    public string? Imdb { get; set; }

    [JsonPropertyName("tmdb")]
    public int? Tmdb { get; set; }
}
