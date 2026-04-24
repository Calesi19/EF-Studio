using EFStudio.Core.Services;
using Xunit;

public class DataServiceTests : TestDatabaseBase
{
    [Fact]
    public async Task GetTableDataAsync_ShouldReturnSeededRows()
    {
        // Arrange
        Context.Users.Add(new TestUser { Id = 1, Name = "Test Admin" });
        await Context.SaveChangesAsync();

        var service = new DataService();

        // Act
        var result = await service.GetTableDataAsync(Context, "Users");

        // Assert
        Assert.NotEmpty(result.Rows);
        // Cast to dictionary because we map to DB column names in the service
        var firstRow = (Dictionary<string, object?>)result.Rows[0];
        Assert.Equal("Test Admin", firstRow["Name"]);
    }
}
