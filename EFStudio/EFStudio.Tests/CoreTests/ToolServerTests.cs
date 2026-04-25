using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using EFStudio.Core.Services;
using EFStudio.Server;

public class ToolServerTests
{
    [Fact]
    public async Task Server_ShouldExposeDiscoveredContexts()
    {
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var projectPath = Path.Combine(solutionRoot, "EFStudio.Sample", "EFStudio.Sample.csproj");
        var loader = new DbContextCatalogLoader();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await using var catalog = await loader.LoadAsync(
            new DbContextDiscoveryOptions(solutionRoot, projectPath, projectPath),
            cancellationTokenSource.Token
        );

        var port = GetAvailablePort();
        var server = new StudioServer();
        await using var handle = await server.StartAsync(
            new StudioServerOptions($"http://127.0.0.1:{port}"),
            catalog,
            cancellationTokenSource.Token
        );

        using var client = new HttpClient();
        var payload = await client.GetStringAsync(
            new Uri(handle.StudioUri.ToString().TrimEnd('/') + "/api/contexts"),
            cancellationTokenSource.Token
        );
        using var document = JsonDocument.Parse(payload);

        var contexts = document.RootElement.GetProperty("contexts");
        Assert.True(contexts.GetArrayLength() >= 1);
        Assert.Contains(
            contexts.EnumerateArray(),
            context => context.GetProperty("name").GetString() == "AppDbContext"
        );
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
