using EFStudio.Core.Services;
using Xunit;

public class SchemaExplorerTests : TestDatabaseBase
{
    [Fact]
    public void GetSchema_ShouldReturnCorrectTableNames()
    {
        // Arrange
        var explorer = new SchemaExplorer();

        // Act
        var schema = explorer.GetSchema(Context);

        // Assert
        var userTable = schema.FirstOrDefault(t => t.Name == "Users");
        Assert.NotNull(userTable);
        Assert.Contains(userTable.Columns, c => c.Name == "Id");
        Assert.Contains(userTable.Columns, c => c.Name == "Name");
    }
}
