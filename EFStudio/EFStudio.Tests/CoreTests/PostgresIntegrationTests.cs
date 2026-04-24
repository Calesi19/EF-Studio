using System.Text.Json;
using EFStudio.Core.Contracts;
using EFStudio.Core.Extensions;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

public class PostgresIntegrationTests(PostgresTestDatabase database) : IClassFixture<PostgresTestDatabase>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task SchemaService_ShouldExposeSchemaQualifiedTableKeys()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await database.Context.Database.EnsureDeletedAsync();
        await database.Context.Database.EnsureCreatedAsync();

        database.Context.CrmUsers.Add(new CrmUser { Id = 1, Name = "Ada Lovelace" });
        database.Context.AuthUsers.Add(new AuthUser { Id = 1, Email = "ada@example.com" });
        database.Context.CrmAuditEntries.Add(new CrmAuditEntry { Id = 100, UserId = 1, EventType = "created" });
        await database.Context.SaveChangesAsync();

        var service = new SchemaService(NullLogger<SchemaService>.Instance);

        var schema = service.GetSchema(database.Context);

        Assert.Contains(schema, table => table.Key == "crm.Users" && table.Schema == "crm");
        Assert.Contains(schema, table => table.Key == "auth.Users" && table.Schema == "auth");

        var auditTable = schema.Single(table => table.Key == "crm.AuditEntries");
        Assert.Contains(
            auditTable.Columns,
            column => column.Name == "UserId" && column.ForeignKeyTable == "crm.Users"
        );
    }

    [Fact]
    public async Task DataService_ShouldReturnRowsForQualifiedPostgresTableKey()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await database.Context.Database.EnsureDeletedAsync();
        await database.Context.Database.EnsureCreatedAsync();

        database.Context.CrmUsers.Add(new CrmUser { Id = 7, Name = "Grace Hopper" });
        database.Context.AuthUsers.Add(new AuthUser { Id = 7, Email = "grace@example.com" });
        await database.Context.SaveChangesAsync();

        var service = new DataService(NullLogger<DataService>.Instance);

        var crmResult = await service.GetTableDataAsync(
            database.Context,
            new TableDataRequestContract("crm.Users"),
            CancellationToken.None
        );

        var authResult = await service.GetTableDataAsync(
            database.Context,
            new TableDataRequestContract("auth.Users"),
            CancellationToken.None
        );

        Assert.NotNull(crmResult);
        Assert.NotNull(authResult);
        Assert.Equal("crm.Users", crmResult.Key);
        Assert.Equal("auth.Users", authResult.Key);
        Assert.Equal("Grace Hopper", crmResult.Rows.Single()["Name"]);
        Assert.Equal("grace@example.com", authResult.Rows.Single()["Email"]);
    }

    [Fact]
    public async Task Middleware_ShouldReturnSchemaQualifiedPayloads_ForPostgres()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await database.Context.Database.EnsureDeletedAsync();
        await database.Context.Database.EnsureCreatedAsync();

        database.Context.CrmUsers.Add(new CrmUser { Id = 9, Name = "Katherine Johnson" });
        await database.Context.SaveChangesAsync();

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddDbContext<PostgresTestDbContext>(options =>
                            options.UseNpgsql(database.ConnectionString));
                        services.AddEFStudio<PostgresTestDbContext>();
                    })
                    .Configure(app => app.UseEFStudio());
            })
            .StartAsync();

        var client = host.GetTestServer().CreateClient();

        var schemaResponse = await client.GetAsync("/efstudio/api/schema");
        var dataResponse = await client.GetAsync("/efstudio/api/data?table=crm.Users");

        schemaResponse.EnsureSuccessStatusCode();
        dataResponse.EnsureSuccessStatusCode();

        var schemaPayload = await schemaResponse.Content.ReadAsStringAsync();
        var tables = JsonSerializer.Deserialize<List<ApiSchemaTable>>(schemaPayload, JsonOptions);

        var dataPayload = await dataResponse.Content.ReadAsStringAsync();
        var tableData = JsonSerializer.Deserialize<ApiTableDataResponse>(dataPayload, JsonOptions);

        Assert.NotNull(tables);
        Assert.Contains(tables, table => table.Key == "crm.Users" && table.Schema == "crm");

        Assert.NotNull(tableData);
        Assert.Equal("crm.Users", tableData.Key);
        Assert.Equal("crm", tableData.Schema);
        Assert.Equal(
            "Katherine Johnson",
            GetJsonValue(tableData.Rows.Single(), "name", "Name").GetString()
        );
    }

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

    private sealed record ApiSchemaTable(string Key, string Name, string? Schema);

    private sealed record ApiTableDataResponse(
        string Key,
        string Name,
        string? Schema,
        List<Dictionary<string, JsonElement>> Rows
    );
}
