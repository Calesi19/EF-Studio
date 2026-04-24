using EFStudio.Core.Contracts;
using EFStudio.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class DataServiceTests : TestDatabaseBase
{
    [Fact]
    public async Task GetTableDataAsync_ShouldReturnSeededRows()
    {
        // Arrange
        Context.Users.Add(new TestUser { Id = 1, Name = "Test Admin" });
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
        Assert.NotEmpty(result.Rows);
        var firstRow = result.Rows[0];
        Assert.Equal("Test Admin", firstRow["Name"]);
    }
}
