using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TopJellyfin;

public class ClientInjector : IHostedService
{
    private const string ScriptTag = "<!-- TopJellyfin --><script src=\"/TopJellyfin/ClientScript\"></script><!-- /TopJellyfin -->";
    private const string MarkerStart = "<!-- TopJellyfin -->";
    private const string MarkerEnd = "<!-- /TopJellyfin -->";

    private readonly IApplicationPaths _appPaths;
    private readonly ILogger<ClientInjector> _logger;
    private string? _indexPath;

    public ClientInjector(IApplicationPaths appPaths, ILogger<ClientInjector> logger)
    {
        _appPaths = appPaths;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _indexPath = FindIndexHtml();
            if (_indexPath == null)
            {
                _logger.LogWarning("Could not find Jellyfin web client index.html. Custom sections will not be injected.");
                return Task.CompletedTask;
            }

            var html = File.ReadAllText(_indexPath);

            if (html.Contains(MarkerStart))
            {
                _logger.LogInformation("TopJellyfin script already injected.");
                return Task.CompletedTask;
            }

            var closingBody = "</body>";
            var insertIndex = html.LastIndexOf(closingBody, StringComparison.OrdinalIgnoreCase);
            if (insertIndex < 0)
            {
                _logger.LogWarning("Could not find </body> tag in index.html.");
                return Task.CompletedTask;
            }

            html = html.Insert(insertIndex, ScriptTag + "\n");
            File.WriteAllText(_indexPath, html);
            _logger.LogInformation("TopJellyfin script injected into {Path}", _indexPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject TopJellyfin script.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_indexPath == null || !File.Exists(_indexPath))
            {
                return Task.CompletedTask;
            }

            var html = File.ReadAllText(_indexPath);
            var startIdx = html.IndexOf(MarkerStart, StringComparison.Ordinal);
            var endIdx = html.IndexOf(MarkerEnd, StringComparison.Ordinal);

            if (startIdx >= 0 && endIdx >= 0)
            {
                endIdx += MarkerEnd.Length;
                // Remove trailing newline if present
                if (endIdx < html.Length && html[endIdx] == '\n')
                {
                    endIdx++;
                }

                html = html.Remove(startIdx, endIdx - startIdx);
                File.WriteAllText(_indexPath, html);
                _logger.LogInformation("TopJellyfin script removed from index.html.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove TopJellyfin script.");
        }

        return Task.CompletedTask;
    }

    private string? FindIndexHtml()
    {
        // Try the standard web path
        var webPath = _appPaths.WebPath;
        if (!string.IsNullOrEmpty(webPath))
        {
            var indexPath = Path.Combine(webPath, "index.html");
            if (File.Exists(indexPath))
            {
                return indexPath;
            }
        }

        return null;
    }
}
