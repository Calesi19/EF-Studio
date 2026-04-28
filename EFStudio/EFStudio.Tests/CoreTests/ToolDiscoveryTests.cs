using EFStudio.Core.Services;

public class ToolDiscoveryTests
{
    [Fact]
    public async Task Loader_ShouldDiscoverSampleDbContext()
    {
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var projectPath = Path.Combine(solutionRoot, "EFStudio.Sample", "EFStudio.Sample.csproj");
        var loader = new DbContextCatalogLoader();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await using var catalog = await loader.LoadAsync(
            new DbContextDiscoveryOptions(solutionRoot, projectPath, projectPath),
            cancellationTokenSource.Token
        );

        var contexts = catalog.GetAvailableContexts();

        Assert.Contains(contexts, context => context.Name == "AppDbContext");
        Assert.Contains(contexts, context => context.IsAvailable);
    }

    // Regression test for the working-directory bug: design-time factories that call
    // Directory.GetCurrentDirectory() to locate appsettings.json failed when EFStudio did
    // not set the working directory to the project directory before invoking CreateDbContext.
    [Fact]
    public async Task Loader_ShouldSetProjectDirectoryBeforeInvokingDesignTimeFactory()
    {
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var projectPath = Path.Combine(solutionRoot, "EFStudio.Sample", "EFStudio.Sample.csproj");
        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectPath))!;
        var loader = new DbContextCatalogLoader();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        var sentinelFile = Path.Combine(Path.GetTempPath(), "efstudio_sentinel_workdir.tmp");
        File.Delete(sentinelFile);

        // Run from a directory that has no appsettings.json to prove EFStudio sets the
        // working directory to the project directory before invoking the factory.
        var originalDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(Path.GetTempPath());
        try
        {
            await using var catalog = await loader.LoadAsync(
                new DbContextDiscoveryOptions(solutionRoot, projectPath, projectPath),
                cancellationTokenSource.Token
            );

            // GetSchemaAsync triggers CreateDbContext on the SentinelDbContextFactory,
            // which always throws after writing the working directory to the sentinel file.
            // The exception here is expected — we only care about the side effect.
            await Assert.ThrowsAnyAsync<Exception>(() =>
                catalog.GetSchemaAsync("SentinelDbContext", cancellationTokenSource.Token));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }

        // The factory wrote its working directory to the sentinel file.
        // If the fix is absent the factory throws FileNotFoundException (appsettings.json
        // not found in the temp dir) before it can write the file.
        Assert.True(File.Exists(sentinelFile), "SentinelDbContextFactory did not write the sentinel file — the factory may not have been invoked or threw before the file write.");

        var recordedDirectory = File.ReadAllText(sentinelFile).Trim();
        Assert.Equal(
            Path.TrimEndingDirectorySeparator(projectDirectory),
            Path.TrimEndingDirectorySeparator(recordedDirectory),
            ignoreCase: true
        );
    }
}
