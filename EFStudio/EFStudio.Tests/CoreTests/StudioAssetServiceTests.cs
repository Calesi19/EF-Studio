using EFStudio.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

public class StudioAssetServiceTests
{
    private static StudioAssetService CreateService() =>
        new(NullLogger<StudioAssetService>.Instance);

    [Fact]
    public void TryOpenAsset_ReturnsTrue_ForKnownEmbeddedFile()
    {
        var service = CreateService();
        var found = service.TryOpenAsset("index.html", out var stream, out _);
        Assert.True(found);
        Assert.NotNull(stream);
        stream?.Dispose();
    }

    [Fact]
    public void TryOpenAsset_ReturnsFalse_ForUnknownFile()
    {
        var service = CreateService();
        var found = service.TryOpenAsset("does-not-exist.js", out var stream, out var contentType);
        Assert.False(found);
        Assert.Null(stream);
        Assert.Equal("application/octet-stream", contentType);
    }

    [Fact]
    public void TryOpenAsset_FallsBackToIndexHtml_WhenPathHasNoExtension()
    {
        var service = CreateService();
        // A path with no extension should fall back to index.html
        var found = service.TryOpenAsset("some/route/without/extension", out var stream, out var contentType);
        // index.html is embedded, so this should succeed
        Assert.True(found);
        Assert.NotNull(stream);
        Assert.Equal("text/html", contentType);
        stream?.Dispose();
    }

    [Fact]
    public void TryOpenAsset_FallsBackToIndexHtml_ForNullPath()
    {
        var service = CreateService();
        var found = service.TryOpenAsset(null, out var stream, out var contentType);
        Assert.True(found);
        Assert.NotNull(stream);
        Assert.Equal("text/html", contentType);
        stream?.Dispose();
    }

    [Fact]
    public void TryOpenAsset_FallsBackToIndexHtml_ForEmptyPath()
    {
        var service = CreateService();
        var found = service.TryOpenAsset("", out var stream, out var contentType);
        Assert.True(found);
        Assert.Equal("text/html", contentType);
        stream?.Dispose();
    }

    [Theory]
    [InlineData("favicon.ico", "image/x-icon")]
    [InlineData("index.html", "text/html")]
    public void TryOpenAsset_SetsCorrectContentType_ForEmbeddedFiles(string path, string expectedContentType)
    {
        var service = CreateService();
        var found = service.TryOpenAsset(path, out var stream, out var contentType);
        Assert.True(found);
        Assert.Equal(expectedContentType, contentType);
        stream?.Dispose();
    }

    [Theory]
    [InlineData("nonexistent.js", "application/octet-stream")]
    [InlineData("nonexistent.css", "application/octet-stream")]
    [InlineData("nonexistent.png", "application/octet-stream")]
    public void TryOpenAsset_SetsOctetStream_WhenFileNotFound(string path, string expectedContentType)
    {
        var service = CreateService();
        var found = service.TryOpenAsset(path, out var stream, out var contentType);
        stream?.Dispose();
        Assert.False(found);
        Assert.Equal(expectedContentType, contentType);
    }
}
