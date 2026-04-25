using System.Text.Json;
using System.Net.Http.Json;
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
                    .UseEnvironment("Development")
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
                    .UseEnvironment("Development")
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
        Assert.Equal("Users", dataResponse.Key);
        Assert.Equal("Users", dataResponse.Name);
        Assert.Equal(fakeUsers.Count, dataResponse.Rows.Count);

        var expectedUser = fakeUsers[0];
        var row = dataResponse.Rows.Single(item => GetJsonValue(item, "id", "Id").GetInt32() == expectedUser.Id);

        Assert.Equal(expectedUser.Name, GetJsonValue(row, "name", "Name").GetString());
        Assert.Equal(expectedUser.Email, GetJsonValue(row, "email", "Email").GetString());
    }

    [Fact]
    public async Task Middleware_ShouldDeleteSelectedRecords()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        connection.Open();

        var fakeUsers = TestDataFactory.CreateUsers(3);

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .UseEnvironment("Development")
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
        var request = new
        {
            tableKey = "Users",
            keys = new[]
            {
                new Dictionary<string, object> { ["Id"] = fakeUsers[0].Id },
                new Dictionary<string, object> { ["Id"] = fakeUsers[1].Id },
            },
        };

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/efstudio/api/data")
        {
            Content = JsonContent.Create(request),
        };

        var response = await client.SendAsync(deleteRequest);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var deleteResponse = JsonSerializer.Deserialize<DeleteRecordsResponse>(payload, JsonOptions);

        Assert.NotNull(deleteResponse);
        Assert.Equal("Users", deleteResponse.TableKey);
        Assert.Equal(2, deleteResponse.DeletedCount);

        var remainingResponse = await client.GetAsync("/efstudio/api/data?table=Users");
        remainingResponse.EnsureSuccessStatusCode();

        var remainingPayload = await remainingResponse.Content.ReadAsStringAsync();
        var remainingData = JsonSerializer.Deserialize<TableDataResponse>(remainingPayload, JsonOptions);

        Assert.NotNull(remainingData);
        Assert.Single(remainingData.Rows);
    }

    [Fact]
    public async Task Middleware_ShouldReturnConflictWhenDeleteViolatesForeignKeys()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Filename=:memory:");
        connection.Open();

        var fakeUser = TestDataFactory.CreateUsers(1).Single();

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .UseEnvironment("Development")
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
                            db.Users.Add(fakeUser);
                            db.UserNotes.Add(new TestUserNote
                            {
                                Id = 1,
                                UserId = fakeUser.Id,
                                Body = "Still referenced",
                            });
                            db.SaveChanges();
                        }

                        app.UseEFStudio();
                    });
            })
            .StartAsync();

        var client = host.GetTestServer().CreateClient();
        var request = new
        {
            tableKey = "Users",
            keys = new[]
            {
                new Dictionary<string, object> { ["Id"] = fakeUser.Id },
            },
        };

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/efstudio/api/data")
        {
            Content = JsonContent.Create(request),
        };

        var response = await client.SendAsync(deleteRequest);

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(payload, JsonOptions);

        Assert.NotNull(error);
        Assert.Contains("still referenced by related data", error.Message);
    }

    private sealed record TableDataResponse(
        string Key,
        string Name,
        string? Schema,
        List<Dictionary<string, JsonElement>> Rows
    );

    private sealed record DeleteRecordsResponse(string TableKey, int DeletedCount);

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
