using EFStudio.Sample.Sqlite.Models;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Sample.Sqlite.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<User>()
            .HasData(
                new Person
                {
                    Id = 1,
                    Name = "Admin User",
                    Email = "admin@efstudio.dev",
                },
                new Person
                {
                    Id = 2,
                    Name = "Dev Tester",
                    Email = "test@example.com",
                }
            );

        modelBuilder
            .Entity<Post>()
            .HasData(
                new Post
                {
                    Id = 1,
                    Title = "Welcome to .NET 10",
                    UserId = 1,
                },
                new Post
                {
                    Id = 2,
                    Title = "EFStudio is Running!",
                    UserId = 2,
                }
            );
    }
}
