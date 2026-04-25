using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace EFStudio.Server;

public sealed class StudioServerHandle : IAsyncDisposable
{
    private readonly WebApplication _app;

    public StudioServerHandle(WebApplication app, Uri baseUri)
    {
        _app = app;
        BaseUri = baseUri;
        StudioUri = new Uri(baseUri, "/efstudio/");
    }

    public Uri BaseUri { get; }
    public Uri StudioUri { get; }

    public Task WaitForShutdownAsync(CancellationToken cancellationToken = default) =>
        _app.WaitForShutdownAsync(cancellationToken);

    public ValueTask DisposeAsync() => _app.DisposeAsync();
}
