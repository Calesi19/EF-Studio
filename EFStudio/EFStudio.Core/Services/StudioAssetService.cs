using Microsoft.Extensions.Logging;

namespace EFStudio.Core.Services;

public sealed class StudioAssetService
{
    private readonly ILogger<StudioAssetService> _logger;

    public StudioAssetService(ILogger<StudioAssetService> logger) => _logger = logger;

    public bool TryOpenAsset(string? assetPath, out Stream? stream, out string contentType)
    {
        var normalizedPath = string.IsNullOrWhiteSpace(assetPath) ? "index.html" : assetPath.TrimStart('/');
        var manifestPath = $"EFStudio.Core.wwwroot.{normalizedPath.Replace("/", ".", StringComparison.Ordinal)}";

        stream = typeof(StudioAssetService).Assembly.GetManifestResourceStream(manifestPath);
        if (stream == null && !Path.HasExtension(normalizedPath))
        {
            normalizedPath = "index.html";
            manifestPath = "EFStudio.Core.wwwroot.index.html";
            stream = typeof(StudioAssetService).Assembly.GetManifestResourceStream(manifestPath);
        }

        if (stream == null)
        {
            _logger.LogWarning("EFStudio embedded asset not found: {ResourcePath}.", manifestPath);
            contentType = "application/octet-stream";
            return false;
        }

        contentType = GetContentType(normalizedPath);
        return true;
    }

    private static string GetContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".js" => "application/javascript",
            ".css" => "text/css",
            ".ico" => "image/x-icon",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".woff2" => "font/woff2",
            ".svg" => "image/svg+xml",
            _ => "text/html",
        };
    }
}
