using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using EFStudio.Contracts;
using EFStudio.Server;
using Microsoft.EntityFrameworkCore;

public class StudioServerErrorTests : TestDatabaseBase
{
    [Fact]
    public async Task Server_ShouldReturn404_ForUnknownTableOnGetData()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/data?table=DoesNotExist"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Server_ShouldReturn400_ForMissingRequiredColumn()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var request = new
        {
            tableKey = "Users",
            records = new[] { new { Email = "only@email.com" } },
        };

        var response = await client.PostAsJsonAsync(new Uri(handle.StudioUri, "api/data"), request);

        // Name is required; missing it throws EFStudioRequestException(400)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Server_ShouldReturn200_ForHealthEndpoint()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/health"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Server_ShouldReturn200_ForTablesEndpoint()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/tables"));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Server_ShouldReturn200_ForSchemaWithContextParam()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/schema?context=TestDbContext"));

        response.EnsureSuccessStatusCode();
    }

    private Task<StudioServerHandle> StartServerAsync()
    {
        var port = GetAvailablePort();
        var server = new StudioServer();
        var catalog = TestDbContextCatalog.CreateSingle(
            "TestDbContext",
            () =>
            {
                var options = new DbContextOptionsBuilder<TestDbContext>()
                    .UseSqlite(Connection)
                    .Options;
                return new TestDbContext(options);
            }
        );

        return server.StartAsync(
            new StudioServerOptions($"http://localhost:{port}"),
            catalog,
            CancellationToken.None
        );
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }
}
