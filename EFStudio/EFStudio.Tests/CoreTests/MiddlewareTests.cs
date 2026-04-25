using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using EFStudio.Server;
using Microsoft.EntityFrameworkCore;

public class StudioServerApiTests : TestDatabaseBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Server_ShouldReturnContextsEndpoint()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/contexts"));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var contextsResponse = JsonSerializer.Deserialize<DbContextsResponse>(payload, JsonOptions);

        Assert.NotNull(contextsResponse);
        Assert.Equal("TestDbContext", contextsResponse.SelectedContext);
        Assert.Contains(
            contextsResponse.Contexts,
            context => context.Name == "TestDbContext" && context.IsSelected && context.IsDefault
        );
    }

    [Fact]
    public async Task Server_ShouldReturnJsonOnSchemaEndpoint()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/schema"));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var tables = JsonSerializer.Deserialize<List<SchemaTableResponse>>(payload, JsonOptions);

        Assert.NotNull(tables);
        var usersTable = tables.Single(table => table.Key == "Users");
        Assert.Equal("Users", usersTable.Name);
        Assert.Null(usersTable.Schema);
    }

    [Fact]
    public async Task Server_ShouldReturnPagedUserDataOnDataEndpoint()
    {
        var fakeUsers = TestDataFactory.CreateUsers(5);
        Context.Users.AddRange(fakeUsers);
        await Context.SaveChangesAsync();

        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/data?table=Users&page=1&pageSize=2"));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var dataResponse = JsonSerializer.Deserialize<TablePageResponse>(payload, JsonOptions);

        Assert.NotNull(dataResponse);
        Assert.Equal("Users", dataResponse.Key);
        Assert.Equal("Users", dataResponse.Name);
        Assert.Equal(1, dataResponse.Page);
        Assert.Equal(2, dataResponse.PageSize);
        Assert.Equal(fakeUsers.Count, dataResponse.TotalRows);
        Assert.Equal(2, dataResponse.Rows.Count);

        var expectedUser = fakeUsers[0];
        var row = dataResponse.Rows.Single(item => GetJsonValue(item, "id", "Id").GetInt32() == expectedUser.Id);

        Assert.Equal(expectedUser.Name, GetJsonValue(row, "name", "Name").GetString());
        Assert.Equal(expectedUser.Email, GetJsonValue(row, "email", "Email").GetString());
    }

    [Fact]
    public async Task Server_ShouldReturnNotFoundForUnknownTable()
    {
        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var response = await client.GetAsync(new Uri(handle.StudioUri, "api/data?table=MissingTable"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(payload, JsonOptions);

        Assert.NotNull(error);
        Assert.Contains("MissingTable", error.Message);
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
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private sealed record DbContextsResponse(
        List<DbContextResponse> Contexts,
        string? SelectedContext
    );

    private sealed record DbContextResponse(
        string Name,
        bool IsSelected,
        bool IsDefault
    );

    private sealed record SchemaTableResponse(
        string Key,
        string Name,
        string? Schema
    );

    private sealed record TablePageResponse(
        string Key,
        string Name,
        string? Schema,
        int Page,
        int PageSize,
        int TotalRows,
        List<Dictionary<string, JsonElement>> Rows
    );

    private sealed record ErrorResponse(string Message);

    private static JsonElement GetJsonValue(
        IReadOnlyDictionary<string, JsonElement> row,
        string camelCaseKey,
        string pascalCaseKey)
    {
        if (row.TryGetValue(camelCaseKey, out var camelCaseValue))
        {
            return camelCaseValue;
        }

        return row[pascalCaseKey];
    }
}
