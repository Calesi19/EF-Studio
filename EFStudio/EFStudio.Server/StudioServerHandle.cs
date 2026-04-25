using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace EFStudio.Server;

public sealed class StudioServerHandle(WebApplication app, Uri studioUri) : IAsyncDisposable
{
    public Uri StudioUri { get; } = studioUri;

    public Task WaitForShutdownAsync(CancellationToken cancellationToken = default) =>
        app.WaitForShutdownAsync(cancellationToken);

    public ValueTask DisposeAsync() => app.DisposeAsync();
}
