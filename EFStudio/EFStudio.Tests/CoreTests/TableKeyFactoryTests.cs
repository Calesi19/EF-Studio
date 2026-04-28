using EFStudio.Core.Services;

public class TableKeyFactoryTests
{
    [Fact]
    public void Create_WithSchema_ReturnsSchemaQualifiedKey()
    {
        var key = TableKeyFactory.Create("crm", "Users");
        Assert.Equal("crm.Users", key);
    }

    [Fact]
    public void Create_WithNullSchema_ReturnsTableNameOnly()
    {
        var key = TableKeyFactory.Create(null, "Users");
        Assert.Equal("Users", key);
    }

    [Fact]
    public void Create_WithEmptySchema_ReturnsTableNameOnly()
    {
        var key = TableKeyFactory.Create("", "Users");
        Assert.Equal("Users", key);
    }

    [Fact]
    public void Create_WithWhitespaceSchema_ReturnsTableNameOnly()
    {
        var key = TableKeyFactory.Create("   ", "Users");
        Assert.Equal("Users", key);
    }

    [Fact]
    public void Create_WithNestedSchema_ReturnsSchemaQualifiedKey()
    {
        var key = TableKeyFactory.Create("hr", "Employees");
        Assert.Equal("hr.Employees", key);
    }
}
