using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace EFStudio.Sample.Sqlite.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var assemblyLocation = typeof(AppDbContextFactory).Assembly.Location;
        var basePath = Path.GetDirectoryName(assemblyLocation)
            ?? throw new InvalidOperationException("EFStudio could not determine the sample assembly directory.");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"EFStudio could not find ConnectionStrings:DefaultConnection in '{basePath}/appsettings.json'."
            );
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString)
            .ReplaceService<IModelCacheKeyFactory, SampleModelCacheKeyFactory>();

        return new AppDbContext(optionsBuilder.Options, configuration);
    }
}
