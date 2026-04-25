using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace EFStudio.Server;

public sealed class StudioServerHandle(WebApplication app, Uri baseUri) : IAsyncDisposable
{
    public Uri BaseUri { get; } = baseUri;
    public Uri StudioUri { get; } = new(baseUri, "/efstudio/");

    public Task WaitForShutdownAsync(CancellationToken cancellationToken = default) =>
        app.WaitForShutdownAsync(cancellationToken);

    public ValueTask DisposeAsync() => app.DisposeAsync();
}
