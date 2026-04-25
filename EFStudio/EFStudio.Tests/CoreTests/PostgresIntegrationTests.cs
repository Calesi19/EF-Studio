using System.Text.Json;
using EFStudio.Core.Contracts;
using EFStudio.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using EFStudio.Server;

public class PostgresIntegrationTests(PostgresTestDatabase database) : IClassFixture<PostgresTestDatabase>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DateOnly CoverageDate = new(2026, 4, 24);
    private static readonly TimeOnly CoverageTime = new(9, 45, 30);
    private static readonly DateTimeOffset CoverageTimestamp = new(2026, 4, 24, 13, 15, 0, TimeSpan.Zero);
    private static readonly Guid CoverageUuid = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly byte[] CoverageBinary = [1, 2, 3, 4, 5, 6];
    private const string CoverageTableKey = "public.TypeCoverage";

    [Fact]
    public async Task SchemaService_ShouldExposeSchemaQualifiedTableKeys()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await ResetDatabaseAsync();

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

        await ResetDatabaseAsync();

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
    public async Task Server_ShouldReturnSchemaQualifiedPayloads_ForPostgres()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await ResetDatabaseAsync();

        database.Context.CrmUsers.Add(new CrmUser { Id = 9, Name = "Katherine Johnson" });
        await database.Context.SaveChangesAsync();

        await using var handle = await StartServerAsync();
        using var client = new HttpClient();

        var schemaResponse = await client.GetAsync(new Uri(handle.StudioUri, "api/schema"));
        var dataResponse = await client.GetAsync(new Uri(handle.StudioUri, "api/data?table=crm.Users"));

        schemaResponse.EnsureSuccessStatusCode();
        dataResponse.EnsureSuccessStatusCode();

        var schemaPayload = await schemaResponse.Content.ReadAsStringAsync();
        var tables = JsonSerializer.Deserialize<List<ApiSchemaTable>>(schemaPayload, JsonOptions);

        var dataPayload = await dataResponse.Content.ReadAsStringAsync();
        var tableData = JsonSerializer.Deserialize<ApiTablePageResponse>(dataPayload, JsonOptions);

        Assert.NotNull(tables);
        Assert.Contains(tables, table => table.Key == "crm.Users" && table.Schema == "crm");

        Assert.NotNull(tableData);
        Assert.Equal("crm.Users", tableData.Key);
        Assert.Equal("crm", tableData.Schema);
        Assert.Equal(1, tableData.Page);
        Assert.Equal(1, tableData.TotalRows);
        Assert.Equal(
            "Katherine Johnson",
            GetJsonValue(tableData.Rows.Single(), "name", "Name").GetString()
        );
    }

    [Fact]
    public async Task SchemaService_ShouldCoverEveryUniquePostgresColumnType()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await ResetDatabaseAsync();

        database.Context.PostgresTypeCoverageRecords.Add(CreateCoverageRecord());
        await database.Context.SaveChangesAsync();

        var service = new SchemaService(NullLogger<SchemaService>.Instance);
        var schema = service.GetSchema(database.Context);

        var table = schema.Single(item => item.Key == CoverageTableKey);
        var columns = table.Columns.ToDictionary(column => column.Name, column => column.DataType);

        Assert.Equal("integer", columns["Id"]);
        Assert.Equal("bigint", columns["BigIntValue"]);
        Assert.Equal("boolean", columns["BooleanValue"]);
        Assert.Equal("bytea", columns["BinaryValue"]);
        Assert.Equal("character varying(80)", columns["VarcharValue"]);
        Assert.Equal("date", columns["DateValue"]);
        Assert.Equal("double precision", columns["DoubleValue"]);
        Assert.Equal("integer", columns["IntValue"]);
        Assert.Equal("jsonb", columns["JsonValue"]);
        Assert.Equal("numeric(18,2)", columns["NumericValue"]);
        Assert.Equal("real", columns["RealValue"]);
        Assert.Equal("smallint", columns["SmallIntValue"]);
        Assert.Equal("text", columns["TextValue"]);
        Assert.Equal("time without time zone", columns["TimeValue"]);
        Assert.Equal("timestamp with time zone", columns["TimestampValue"]);
        Assert.Equal("uuid", columns["UuidValue"]);
    }

    [Fact]
    public async Task Server_ShouldSerializeEveryUniquePostgresColumnType()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await ResetDatabaseAsync();

        database.Context.PostgresTypeCoverageRecords.Add(CreateCoverageRecord());
        await database.Context.SaveChangesAsync();

        await using var handle = await StartServerAsync();
        using var client = new HttpClient();
        var response = await client.GetAsync(new Uri(handle.StudioUri, $"api/data?table={CoverageTableKey}"));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var tableData = JsonSerializer.Deserialize<ApiTablePageResponse>(payload, JsonOptions);

        Assert.NotNull(tableData);
        var row = tableData.Rows.Single();

        Assert.Equal(1, GetJsonValue(row, "id", "Id").GetInt32());
        Assert.Equal(9_000_000_001L, GetJsonValue(row, "bigIntValue", "BigIntValue").GetInt64());
        Assert.True(GetJsonValue(row, "booleanValue", "BooleanValue").GetBoolean());
        Assert.Equal(Convert.ToBase64String(CoverageBinary), GetJsonValue(row, "binaryValue", "BinaryValue").GetString());
        Assert.Equal("varchar sample", GetJsonValue(row, "varcharValue", "VarcharValue").GetString());
        Assert.Equal("2026-04-24", GetJsonValue(row, "dateValue", "DateValue").GetString());
        Assert.Equal(42.125d, GetJsonValue(row, "doubleValue", "DoubleValue").GetDouble(), 6);
        Assert.Equal(73, GetJsonValue(row, "intValue", "IntValue").GetInt32());
        var jsonValue = GetJsonValue(row, "jsonValue", "JsonValue").GetString();
        Assert.NotNull(jsonValue);
        using (var document = JsonDocument.Parse(jsonValue))
        {
            Assert.True(document.RootElement.GetProperty("flag").GetBoolean());
            Assert.Equal("pg", document.RootElement.GetProperty("tags")[0].GetString());
            Assert.Equal("jsonb", document.RootElement.GetProperty("tags")[1].GetString());
        }
        Assert.Equal(1234.56m, GetJsonValue(row, "numericValue", "NumericValue").GetDecimal());
        Assert.Equal(7.5f, GetJsonValue(row, "realValue", "RealValue").GetSingle(), 3);
        Assert.Equal(12, GetJsonValue(row, "smallIntValue", "SmallIntValue").GetInt16());
        Assert.Equal("text sample", GetJsonValue(row, "textValue", "TextValue").GetString());
        Assert.StartsWith("09:45:30", GetJsonValue(row, "timeValue", "TimeValue").GetString());
        Assert.Equal("2026-04-24T13:15:00+00:00", GetJsonValue(row, "timestampValue", "TimestampValue").GetString());
        Assert.Equal(CoverageUuid.ToString(), GetJsonValue(row, "uuidValue", "UuidValue").GetString());
    }

    [Fact]
    public async Task DataService_ShouldDeleteRowsForQualifiedPostgresTableKey()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await ResetDatabaseAsync();

        database.Context.CrmUsers.AddRange(
            new CrmUser { Id = 21, Name = "Delete One" },
            new CrmUser { Id = 22, Name = "Delete Two" },
            new CrmUser { Id = 23, Name = "Keep Three" }
        );
        await database.Context.SaveChangesAsync();

        var service = new DataService(NullLogger<DataService>.Instance);

        var result = await service.DeleteRecordsAsync(
            database.Context,
            new DeleteRecordsRequestContract(
                "crm.Users",
                [
                    CreateKeyValues(("Id", 21)),
                    CreateKeyValues(("Id", 22)),
                ]
            ),
            CancellationToken.None
        );

        Assert.Equal("crm.Users", result.TableKey);
        Assert.Equal(2, result.DeletedCount);
        Assert.Equal(new[] { 23 }, database.Context.CrmUsers.Select(user => user.Id).ToArray());
    }

    [Fact]
    public async Task DataService_ShouldFailTransactionallyWhenDeleteViolatesForeignKeys()
    {
        if (!database.IsAvailable())
        {
            return;
        }

        await ResetDatabaseAsync();

        database.Context.CrmUsers.AddRange(
            new CrmUser { Id = 31, Name = "Blocked User" },
            new CrmUser { Id = 32, Name = "Should Remain" }
        );
        database.Context.CrmAuditEntries.Add(
            new CrmAuditEntry { Id = 501, UserId = 31, EventType = "created" }
        );
        await database.Context.SaveChangesAsync();

        var service = new DataService(NullLogger<DataService>.Instance);

        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            service.DeleteRecordsAsync(
                database.Context,
                new DeleteRecordsRequestContract(
                    "crm.Users",
                    [
                        CreateKeyValues(("Id", 31)),
                        CreateKeyValues(("Id", 32)),
                    ]
                ),
                CancellationToken.None
            )
        );

        Assert.Contains("still referenced by related data", exception.Message);
        Assert.Equal(
            new[] { 31, 32 },
            database.Context.CrmUsers.OrderBy(user => user.Id).Select(user => user.Id).ToArray()
        );
    }

    private static PostgresTypeCoverageRecord CreateCoverageRecord()
    {
        return new PostgresTypeCoverageRecord
        {
            Id = 1,
            BigIntValue = 9_000_000_001L,
            BooleanValue = true,
            BinaryValue = CoverageBinary,
            VarcharValue = "varchar sample",
            DateValue = CoverageDate,
            DoubleValue = 42.125d,
            IntValue = 73,
            JsonValue = """{"flag":true,"tags":["pg","jsonb"]}""",
            NumericValue = 1234.56m,
            RealValue = 7.5f,
            SmallIntValue = 12,
            TextValue = "text sample",
            TimeValue = CoverageTime,
            TimestampValue = CoverageTimestamp,
            UuidValue = CoverageUuid,
        };
    }

    private async Task ResetDatabaseAsync()
    {
        database.Context.ChangeTracker.Clear();
        await database.Context.Database.EnsureDeletedAsync();
        database.Context.ChangeTracker.Clear();
        await database.Context.Database.EnsureCreatedAsync();
        database.Context.ChangeTracker.Clear();
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

    private static IReadOnlyDictionary<string, JsonElement> CreateKeyValues(params (string Key, object Value)[] pairs)
    {
        return pairs.ToDictionary(
            pair => pair.Key,
            pair => JsonSerializer.SerializeToElement(pair.Value)
        );
    }

    private Task<StudioServerHandle> StartServerAsync()
    {
        var port = GetAvailablePort();
        var server = new StudioServer();
        var catalog = TestDbContextCatalog.CreateSingle(
            nameof(PostgresTestDbContext),
            () =>
            {
                var options = new DbContextOptionsBuilder<PostgresTestDbContext>()
                    .UseNpgsql(database.ConnectionString)
                    .Options;
                return new PostgresTestDbContext(options);
            }
        );

        return server.StartAsync(
            new StudioServerOptions($"http://127.0.0.1:{port}"),
            catalog,
            CancellationToken.None
        );
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }

    private sealed record ApiSchemaTable(string Key, string Name, string? Schema);

    private sealed record ApiTablePageResponse(
        string Key,
        string Name,
        string? Schema,
        int Page,
        int PageSize,
        int TotalRows,
        List<Dictionary<string, JsonElement>> Rows
    );
}
