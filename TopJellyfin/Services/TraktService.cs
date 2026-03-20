using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TopJellyfin.Configuration;
using TopJellyfin.Models;

namespace TopJellyfin.Services;

public class TraktService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TraktService> _logger;

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public TraktService(IHttpClientFactory httpClientFactory, ILogger<TraktService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<TraktMovieResult>> GetTrendingMoviesAsync()
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var endpoint = config.TraktMoviesEndpoint;
        var cacheKey = $"movies/{endpoint}";

        if (TryGetCached<List<TraktMovieResult>>(cacheKey, out var cached))
        {
            return cached!;
        }

        var url = $"https://api.trakt.tv/movies/{endpoint}?extended=full&limit=50";
        var results = await FetchFromTraktAsync<List<TraktMovieResult>>(url).ConfigureAwait(false);

        if (results != null)
        {
            SetCache(cacheKey, results);
        }

        return results ?? new List<TraktMovieResult>();
    }

    public async Task<List<TraktShowResult>> GetTrendingShowsAsync()
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        var endpoint = config.TraktShowsEndpoint;
        var cacheKey = $"shows/{endpoint}";

        if (TryGetCached<List<TraktShowResult>>(cacheKey, out var cached))
        {
            return cached!;
        }

        var url = $"https://api.trakt.tv/shows/{endpoint}?extended=full&limit=50";
        var results = await FetchFromTraktAsync<List<TraktShowResult>>(url).ConfigureAwait(false);

        if (results != null)
        {
            SetCache(cacheKey, results);
        }

        return results ?? new List<TraktShowResult>();
    }

    private async Task<T?> FetchFromTraktAsync<T>(string url) where T : class
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();

        if (string.IsNullOrWhiteSpace(config.TraktClientId))
        {
            _logger.LogWarning("Trakt Client ID is not configured. Skipping Trakt API call.");
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("TopJellyfin");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("trakt-api-key", config.TraktClientId);
            request.Headers.Add("trakt-api-version", "2");

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching from Trakt API: {Url}", url);
            return null;
        }
    }

    private bool TryGetCached<T>(string key, out T? value) where T : class
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
        {
            value = entry.Data as T;
            return value != null;
        }

        value = null;
        return false;
    }

    private void SetCache(string key, object data)
    {
        var config = Plugin.Instance?.Configuration ?? new PluginConfiguration();
        _cache[key] = new CacheEntry
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(config.CacheDurationMinutes),
            Data = data
        };
    }

    private class CacheEntry
    {
        public DateTime ExpiresAt { get; set; }

        public object Data { get; set; } = null!;
    }
}
