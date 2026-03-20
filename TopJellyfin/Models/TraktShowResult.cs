using System.Text.Json.Serialization;

namespace TopJellyfin.Models;

public class TraktShowResult
{
    [JsonPropertyName("watchers")]
    public int Watchers { get; set; }

    [JsonPropertyName("show")]
    public TraktShow? Show { get; set; }
}

public class TraktShow
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("ids")]
    public TraktIds? Ids { get; set; }
}
