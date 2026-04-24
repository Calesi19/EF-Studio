using System.Text.Json;
using EFStudio.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

public class MiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Middleware_ShouldReturnJsonOnSchemaEndpoint()
    {
        // 1. Create a SQLite connection that stays open for the duration of the test
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        connection.Open();

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        // 2. Use SQLite instead of the InMemory provider
                        services.AddDbContext<TestDbContext>(opt => opt.UseSqlite(connection));
                        services.AddEFStudio<TestDbContext>();
                    })
                    .Configure(app =>
                    {
                        // Ensure the schema is created in our in-memory SQLite
                        using (var scope = app.ApplicationServices.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
                            db.Database.EnsureCreated();
                        }

                        app.UseEFStudio();
                    });
            })
            .StartAsync();

        var client = host.GetTestServer().CreateClient();

        // Act
        var response = await client.GetAsync("/efstudio/api/schema");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Users", content);
    }

    [Fact]
    public async Task Middleware_ShouldReturnBogusUserDataOnDataEndpoint()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        connection.Open();

        var fakeUsers = TestDataFactory.CreateUsers(5);

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddDbContext<TestDbContext>(opt => opt.UseSqlite(connection));
                        services.AddEFStudio<TestDbContext>();
                    })
                    .Configure(app =>
                    {
                        using (var scope = app.ApplicationServices.CreateScope())
                        {
                            var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();
                            db.Database.EnsureCreated();
                            db.Users.AddRange(fakeUsers);
                            db.SaveChanges();
                        }

                        app.UseEFStudio();
                    });
            })
            .StartAsync();

        var client = host.GetTestServer().CreateClient();

        var response = await client.GetAsync("/efstudio/api/data?table=Users");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var dataResponse = JsonSerializer.Deserialize<TableDataResponse>(payload, JsonOptions);

        Assert.NotNull(dataResponse);
        Assert.Equal("Users", dataResponse.Name);
        Assert.Equal(fakeUsers.Count, dataResponse.Rows.Count);

        var expectedUser = fakeUsers[0];
        var row = dataResponse.Rows.Single(item => GetJsonValue(item, "id", "Id").GetInt32() == expectedUser.Id);

        Assert.Equal(expectedUser.Name, GetJsonValue(row, "name", "Name").GetString());
        Assert.Equal(expectedUser.Email, GetJsonValue(row, "email", "Email").GetString());
    }

    private sealed record TableDataResponse(string Name, List<Dictionary<string, JsonElement>> Rows);

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
