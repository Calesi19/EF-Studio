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
        Assert.Equal(fakeUsers.Count, result.Rows.Count);

        var firstExpectedUser = fakeUsers[0];
        var firstRow = result.Rows.Single(row => Convert.ToInt32(row["Id"]) == firstExpectedUser.Id);

        Assert.Equal(firstExpectedUser.Name, firstRow["Name"]);
        Assert.Equal(firstExpectedUser.Email, firstRow["Email"]);
        Assert.Equal(firstExpectedUser.IsActive, firstRow["IsActive"]);
        Assert.Equal(firstExpectedUser.CreditLimit, firstRow["CreditLimit"]);
    }
}
