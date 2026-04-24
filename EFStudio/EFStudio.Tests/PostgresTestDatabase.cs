using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

public sealed class PostgresTestDatabase : IAsyncLifetime, IDisposable
{
    private PostgreSqlContainer? _container;
    private PostgresTestDbContext? _context;

    public PostgresTestDbContext Context =>
        _context ?? throw new InvalidOperationException("PostgreSQL test context is not initialized.");

    public string ConnectionString =>
        _container?.GetConnectionString()
        ?? throw new InvalidOperationException("PostgreSQL test container is not initialized.");

    public Exception? InitializationException { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("efstudio_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync();

            var options = new DbContextOptionsBuilder<PostgresTestDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;

            _context = new PostgresTestDbContext(options);
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception exception)
        {
            InitializationException = exception;
        }
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    public bool IsAvailable()
    {
        return InitializationException == null;
    }
}

public class PostgresTestDbContext(DbContextOptions<PostgresTestDbContext> options) : DbContext(options)
{
    public DbSet<CrmUser> CrmUsers => Set<CrmUser>();
    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<CrmAuditEntry> CrmAuditEntries => Set<CrmAuditEntry>();
    public DbSet<PostgresTypeCoverageRecord> PostgresTypeCoverageRecords => Set<PostgresTypeCoverageRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CrmUser>(entity =>
        {
            entity.ToTable("Users", "crm");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<AuthUser>(entity =>
        {
            entity.ToTable("Users", "auth");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(160);
        });

        modelBuilder.Entity<CrmAuditEntry>(entity =>
        {
            entity.ToTable("AuditEntries", "crm");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(40);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PostgresTypeCoverageRecord>(entity =>
        {
            entity.ToTable("TypeCoverage", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VarcharValue).HasMaxLength(80);
            entity.Property(x => x.JsonValue).HasColumnType("jsonb");
            entity.Property(x => x.NumericValue).HasPrecision(18, 2);
            entity.Property(x => x.TextValue).HasColumnType("text");
        });
    }
}

public class CrmUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class AuthUser
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
}

public class CrmAuditEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string EventType { get; set; } = "";
    public CrmUser User { get; set; } = null!;
}

public class PostgresTypeCoverageRecord
{
    public int Id { get; set; }
    public long BigIntValue { get; set; }
    public bool BooleanValue { get; set; }
    public byte[] BinaryValue { get; set; } = [];
    public string VarcharValue { get; set; } = "";
    public DateOnly DateValue { get; set; }
    public double DoubleValue { get; set; }
    public int IntValue { get; set; }
    public string JsonValue { get; set; } = "";
    public decimal NumericValue { get; set; }
    public float RealValue { get; set; }
    public short SmallIntValue { get; set; }
    public string TextValue { get; set; } = "";
    public TimeOnly TimeValue { get; set; }
    public DateTimeOffset TimestampValue { get; set; }
    public Guid UuidValue { get; set; }
}
