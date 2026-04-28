using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EFStudio.Sample.Sqlite.Data;

// Tests that EFStudio sets the working directory to the project directory before
// invoking design-time factories. Uses Directory.GetCurrentDirectory() to locate
// appsettings.json, which is the pattern that triggered the original bug.
public class SentinelDbContextFactory : IDesignTimeDbContextFactory<SentinelDbContext>
{
    public const string SentinelFileName = "efstudio_sentinel_workdir.tmp";

    public SentinelDbContext CreateDbContext(string[] args)
    {
        var workingDirectory = Directory.GetCurrentDirectory();

        // Write first so the test can observe the working directory even if the
        // subsequent appsettings read fails (which would indicate the wrong directory).
        File.WriteAllText(Path.Combine(Path.GetTempPath(), SentinelFileName), workingDirectory);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(workingDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("SentinelConnection")
            ?? throw new InvalidOperationException(
                "SentinelConnection not found in appsettings.json."
            );

        // Throw a known sentinel so the test can distinguish "factory ran and recorded
        // the directory" from "factory never ran" without needing a working DB provider.
        throw new InvalidOperationException(
            $"SentinelDbContextFactory reached end with connection string '{connectionString}'."
        );
    }
}
