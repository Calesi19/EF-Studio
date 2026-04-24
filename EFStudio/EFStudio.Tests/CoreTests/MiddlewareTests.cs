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
}
