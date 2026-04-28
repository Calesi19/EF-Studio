using EFStudio.Core.Extensions;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;

public class ServiceCollectionExtensionsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestDbContext _context;

    public ServiceCollectionExtensionsTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options;
        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void AddEFStudio_RegistersISchemaService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o => o.UseSqlite(_connection));
        services.AddEFStudio<TestDbContext>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();

        var schemaService = scope.ServiceProvider.GetService<ISchemaService>();
        Assert.NotNull(schemaService);
    }

    [Fact]
    public void AddEFStudio_RegistersIDataService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o => o.UseSqlite(_connection));
        services.AddEFStudio<TestDbContext>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();

        var dataService = scope.ServiceProvider.GetService<IDataService>();
        Assert.NotNull(dataService);
    }

    [Fact]
    public void AddEFStudio_RegistersStudioAssetService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o => o.UseSqlite(_connection));
        services.AddEFStudio<TestDbContext>();

        var provider = services.BuildServiceProvider();
        var assetService = provider.GetService<StudioAssetService>();
        Assert.NotNull(assetService);
    }

    [Fact]
    public void AddEFStudio_RegistersDbContext_ViaBaseType()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(o => o.UseSqlite(_connection));
        services.AddEFStudio<TestDbContext>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetService<DbContext>();
        Assert.NotNull(dbContext);
        Assert.IsType<TestDbContext>(dbContext);
    }

    [Fact]
    public void UseEFStudio_ThrowsInNonDevelopmentEnvironment()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Production";
        builder.Services.AddLogging();
        builder.Services.AddDbContext<TestDbContext>(o => o.UseSqlite(_connection));
        builder.Services.AddEFStudio<TestDbContext>();

        var app = builder.Build();

        Assert.Throws<InvalidOperationException>(() => app.UseEFStudio());
    }

    [Fact]
    public void UseEFStudio_WithForceTrue_DoesNotThrowInProduction()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = "Production";
        builder.Services.AddLogging();
        builder.Services.AddDbContext<TestDbContext>(o => o.UseSqlite(_connection));
        builder.Services.AddEFStudio<TestDbContext>();

        var app = builder.Build();
        var exception = Record.Exception(() => app.UseEFStudio(force: true));
        Assert.Null(exception);
    }
}
