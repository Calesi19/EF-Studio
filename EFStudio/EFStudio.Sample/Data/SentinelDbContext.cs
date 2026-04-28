using Microsoft.EntityFrameworkCore;

namespace EFStudio.Sample.Sqlite.Data;

public class SentinelDbContext : DbContext
{
    public SentinelDbContext(DbContextOptions<SentinelDbContext> options) : base(options) { }
    public DbSet<SentinelMarker> Markers => Set<SentinelMarker>();
}

public class SentinelMarker
{
    public int Id { get; set; }
}
