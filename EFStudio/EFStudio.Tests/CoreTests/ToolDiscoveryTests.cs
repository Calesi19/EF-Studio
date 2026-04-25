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
}
