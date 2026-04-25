using Bogus;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public abstract class TestDatabaseBase : IDisposable
{
    protected SqliteConnection Connection { get; }
    protected readonly TestDbContext Context;

    protected TestDatabaseBase()
    {
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(Connection).Options;

        Context = new TestDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}

// A simple context just for testing
public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestUser> Users => Set<TestUser>();
    public DbSet<TestUserNote> UserNotes => Set<TestUserNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestUserNote>(entity =>
        {
            entity.HasKey(note => note.Id);
            entity.HasOne(note => note.User)
                .WithMany()
                .HasForeignKey(note => note.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public class TestUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public decimal CreditLimit { get; set; }
}

public class TestUserNote
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Body { get; set; } = "";
    public TestUser User { get; set; } = null!;
}

public static class TestDataFactory
{
    private const int BogusSeed = 424242;

    public static List<TestUser> CreateUsers(int count)
    {
        Randomizer.Seed = new Random(BogusSeed);

        var nextId = 1;

        return new Faker<TestUser>()
            .RuleFor(user => user.Id, _ => nextId++)
            .RuleFor(user => user.Name, faker => faker.Name.FullName())
            .RuleFor(user => user.Email, (faker, user) => faker.Internet.Email(user.Name.Replace(" ", ".")))
            .RuleFor(user => user.IsActive, faker => faker.Random.Bool(0.8f))
            .RuleFor(user => user.CreatedAtUtc, faker => faker.Date.Past(2).ToUniversalTime())
            .RuleFor(user => user.CreditLimit, faker => faker.Random.Decimal(500m, 25_000m))
            .Generate(count);
    }
}
