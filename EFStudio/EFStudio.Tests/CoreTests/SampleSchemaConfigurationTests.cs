using EFStudio.Sample.Sqlite.Data;
using EFStudio.Sample.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

public class SampleSchemaConfigurationTests
{
    [Fact]
    public void AppDbContext_ShouldKeepDefaultSchemas_WhenToggleIsDisabled()
    {
        using var context = CreateContext(useSchemas: false);

        Assert.Null(context.Model.FindEntityType(typeof(Customer))?.GetSchema());
        Assert.Null(context.Model.FindEntityType(typeof(Employee))?.GetSchema());
        Assert.Null(context.Model.FindEntityType(typeof(Project))?.GetSchema());
    }

    [Fact]
    public void AppDbContext_ShouldApplySampleSchemas_WhenToggleIsEnabled()
    {
        using var context = CreateContext(useSchemas: true);

        Assert.Equal("crm", context.Model.FindEntityType(typeof(Customer))?.GetSchema());
        Assert.Equal("hr", context.Model.FindEntityType(typeof(Employee))?.GetSchema());
        Assert.Equal("ops", context.Model.FindEntityType(typeof(Project))?.GetSchema());
        Assert.Null(context.Model.FindEntityType(typeof(Company))?.GetSchema());
    }

    private static AppDbContext CreateContext(bool useSchemas)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["SampleData:UseSchemas"] = useSchemas.ToString(),
                })
            .Build();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=efstudio_sample_tests;Username=postgres;Password=postgres")
            .ReplaceService<IModelCacheKeyFactory, SampleModelCacheKeyFactory>()
            .Options;

        return new AppDbContext(options, configuration);
    }
}
