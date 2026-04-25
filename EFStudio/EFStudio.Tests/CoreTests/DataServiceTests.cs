using System.Text.Json;
using EFStudio.Core.Contracts;
using EFStudio.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class DataServiceTests : TestDatabaseBase
{
    [Fact]
    public async Task GetTableDataAsync_ShouldReturnBogusRows()
    {
        // Arrange
        var fakeUsers = TestDataFactory.CreateUsers(12);
        Context.Users.AddRange(fakeUsers);
        await Context.SaveChangesAsync();

        var service = new DataService(NullLogger<DataService>.Instance);

        // Act
        var result = await service.GetTableDataAsync(
            Context,
            new TableDataRequestContract("Users"),
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Users", result.Key);
        Assert.Equal(fakeUsers.Count, result.Rows.Count);

        var firstExpectedUser = fakeUsers[0];
        var firstRow = result.Rows.Single(row => Convert.ToInt32(row["Id"]) == firstExpectedUser.Id);

        Assert.Equal(firstExpectedUser.Name, firstRow["Name"]);
        Assert.Equal(firstExpectedUser.Email, firstRow["Email"]);
        Assert.Equal(firstExpectedUser.IsActive, firstRow["IsActive"]);
        Assert.Equal(firstExpectedUser.CreditLimit, firstRow["CreditLimit"]);
    }

    [Fact]
    public async Task DeleteRecordsAsync_ShouldDeleteSelectedRows()
    {
        var fakeUsers = TestDataFactory.CreateUsers(4);
        Context.Users.AddRange(fakeUsers);
        await Context.SaveChangesAsync();

        var service = new DataService(NullLogger<DataService>.Instance);

        var result = await service.DeleteRecordsAsync(
            Context,
            new DeleteRecordsRequestContract(
                "Users",
                [
                    CreateKeyValues(("Id", fakeUsers[0].Id)),
                    CreateKeyValues(("Id", fakeUsers[2].Id)),
                ]
            ),
            CancellationToken.None
        );

        Assert.Equal("Users", result.TableKey);
        Assert.Equal(2, result.DeletedCount);
        Assert.Equal(2, Context.Users.Count());
        Assert.DoesNotContain(Context.Users, user => user.Id == fakeUsers[0].Id);
        Assert.DoesNotContain(Context.Users, user => user.Id == fakeUsers[2].Id);
    }

    [Fact]
    public async Task DeleteRecordsAsync_ShouldFailForMissingPrimaryKeyColumn()
    {
        var fakeUsers = TestDataFactory.CreateUsers(1);
        Context.Users.AddRange(fakeUsers);
        await Context.SaveChangesAsync();

        var service = new DataService(NullLogger<DataService>.Instance);

        var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            service.DeleteRecordsAsync(
                Context,
                new DeleteRecordsRequestContract(
                    "Users",
                    [
                        CreateKeyValues(("Email", fakeUsers[0].Email)),
                    ]
                ),
                CancellationToken.None
            )
        );

        Assert.Equal(
            "Delete requests for 'Users' must include the primary key column 'Id'.",
            exception.Message
        );
    }

    private static IReadOnlyDictionary<string, JsonElement> CreateKeyValues(params (string Key, object Value)[] pairs)
    {
        return pairs.ToDictionary(
            pair => pair.Key,
            pair => JsonSerializer.SerializeToElement(pair.Value)
        );
    }
}
