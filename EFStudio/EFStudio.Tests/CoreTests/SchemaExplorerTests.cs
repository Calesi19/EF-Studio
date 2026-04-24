using EFStudio.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class SchemaExplorerTests : TestDatabaseBase
{
    [Fact]
    public void GetSchema_ShouldReturnCorrectTableNames()
    {
        // Arrange
        var service = new SchemaService(NullLogger<SchemaService>.Instance);

        // Act
        var schema = service.GetSchema(Context);

        // Assert
        var userTable = schema.FirstOrDefault(t => t.Name == "Users");
        Assert.NotNull(userTable);
        Assert.Contains(userTable.Columns, c => c.Name == "Id");
        Assert.Contains(userTable.Columns, c => c.Name == "Name");
    }
}
