using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public abstract class TestDatabaseBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly TestDbContext Context;

    protected TestDatabaseBase()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options;

        Context = new TestDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}

// A simple context just for testing
public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestUser> Users => Set<TestUser>();
}

public class TestUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
