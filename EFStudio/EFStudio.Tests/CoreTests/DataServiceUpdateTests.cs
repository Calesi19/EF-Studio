using System.Text.Json;
using EFStudio.Contracts;
using EFStudio.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

public class DataServiceUpdateTests : TestDatabaseBase
{
    private static DataService CreateService() =>
        new(NullLogger<DataService>.Instance);

    [Fact]
    public async Task UpdateRecordsAsync_ShouldModifyFieldValues()
    {
        var user = new TestUser { Name = "Alice", Email = "alice@example.com", IsActive = true };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var service = CreateService();
        var result = await service.UpdateRecordsAsync(
            Context,
            new UpdateRecordsRequestContract(
                "Users",
                [new UpdateRecordEntry(
                    Keys: CreateKeyValues(("Id", user.Id)),
                    Values: CreateKeyValues(("Name", "Alicia"))
                )]
            ),
            CancellationToken.None
        );

        Assert.Equal("Users", result.TableKey);
        Assert.Equal(1, result.UpdatedCount);

        var updated = Context.Users.Single(u => u.Id == user.Id);
        Assert.Equal("Alicia", updated.Name);
    }

    [Fact]
    public async Task UpdateRecordsAsync_ShouldThrow_WhenRecordNotFound()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            service.UpdateRecordsAsync(
                Context,
                new UpdateRecordsRequestContract(
                    "Users",
                    [new UpdateRecordEntry(
                        Keys: CreateKeyValues(("Id", 99999)),
                        Values: CreateKeyValues(("Name", "Ghost"))
                    )]
                ),
                CancellationToken.None
            )
        );

        Assert.Contains("could not be found", exception.Message);
    }

    [Fact]
    public async Task UpdateRecordsAsync_ShouldThrow_WhenTableKeyIsEmpty()
    {
        var service = CreateService();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.UpdateRecordsAsync(
                Context,
                new UpdateRecordsRequestContract(
                    "",
                    [new UpdateRecordEntry(
                        Keys: CreateKeyValues(("Id", 1)),
                        Values: CreateKeyValues(("Name", "X"))
                    )]
                ),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task UpdateRecordsAsync_ShouldThrow_WhenNoUpdatesProvided()
    {
        var service = CreateService();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.UpdateRecordsAsync(
                Context,
                new UpdateRecordsRequestContract("Users", []),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task UpdateRecordsAsync_ShouldSkipPrimaryKeyColumn()
    {
        var user = new TestUser { Name = "Bob", Email = "bob@test.com", IsActive = false };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        var service = CreateService();
        var originalId = user.Id;

        var result = await service.UpdateRecordsAsync(
            Context,
            new UpdateRecordsRequestContract(
                "Users",
                [new UpdateRecordEntry(
                    Keys: CreateKeyValues(("Id", user.Id)),
                    Values: CreateKeyValues(("Id", 99999), ("Name", "Bobby"))
                )]
            ),
            CancellationToken.None
        );

        Assert.Equal(1, result.UpdatedCount);
        var updated = Context.Users.Single(u => u.Id == originalId);
        Assert.Equal(originalId, updated.Id);
        Assert.Equal("Bobby", updated.Name);
    }

    [Fact]
    public async Task GetTablePageAsync_ShouldPaginateCorrectly()
    {
        var users = Enumerable.Range(1, 15)
            .Select(i => new TestUser { Name = $"User{i:D2}", Email = $"u{i}@test.com" })
            .ToList();
        Context.Users.AddRange(users);
        await Context.SaveChangesAsync();

        var service = CreateService();

        var page1 = await service.GetTablePageAsync(
            Context,
            new TablePageRequestContract("Users", 1, 5),
            CancellationToken.None
        );
        var page2 = await service.GetTablePageAsync(
            Context,
            new TablePageRequestContract("Users", 2, 5),
            CancellationToken.None
        );

        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.Equal(5, page1!.Rows.Count);
        Assert.Equal(5, page2!.Rows.Count);
        Assert.Equal(15, page1.TotalRows);
        Assert.Equal(1, page1.Page);
        Assert.Equal(2, page2.Page);
    }

    [Fact]
    public async Task GetTablePageAsync_ShouldFilterRows()
    {
        Context.Users.AddRange(
            new TestUser { Name = "Alice Smith", Email = "alice@test.com" },
            new TestUser { Name = "Bob Jones", Email = "bob@test.com" }
        );
        await Context.SaveChangesAsync();

        var service = CreateService();
        var result = await service.GetTablePageAsync(
            Context,
            new TablePageRequestContract("Users", 1, 50, "Alice"),
            CancellationToken.None
        );

        Assert.NotNull(result);
        Assert.Equal(1, result!.TotalRows);
        var row = Assert.Single(result.Rows);
        Assert.Contains("Alice", row["Name"]?.ToString());
    }

    [Fact]
    public async Task GetTablePageAsync_ShouldSortAscending()
    {
        Context.Users.AddRange(
            new TestUser { Name = "Charlie", Email = "c@test.com" },
            new TestUser { Name = "Alice", Email = "a@test.com" },
            new TestUser { Name = "Bob", Email = "b@test.com" }
        );
        await Context.SaveChangesAsync();

        var service = CreateService();
        var result = await service.GetTablePageAsync(
            Context,
            new TablePageRequestContract("Users", 1, 50, null, "Name", "asc"),
            CancellationToken.None
        );

        Assert.NotNull(result);
        var names = result!.Rows.Select(r => r["Name"]?.ToString()).ToList();
        Assert.Equal(["Alice", "Bob", "Charlie"], names);
    }

    [Fact]
    public async Task GetTablePageAsync_ReturnsNull_ForUnknownTable()
    {
        var service = CreateService();
        var result = await service.GetTablePageAsync(
            Context,
            new TablePageRequestContract("UnknownTable", 1, 10),
            CancellationToken.None
        );

        Assert.Null(result);
    }

    private static IReadOnlyDictionary<string, JsonElement> CreateKeyValues(params (string Key, object Value)[] pairs)
    {
        return pairs.ToDictionary(
            pair => pair.Key,
            pair => JsonSerializer.SerializeToElement(pair.Value)
        );
    }
}
