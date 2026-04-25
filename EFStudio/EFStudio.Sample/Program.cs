using EFStudio.Sample.Sqlite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ReplaceService<IModelCacheKeyFactory, SampleModelCacheKeyFactory>()
);
var app = builder.Build();

// 3. Auto-create database for the demo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    await SampleDataSeeder.SeedAsync(db);
}

app.MapGet("/", () => Results.Ok(new { status = "EFStudio sample API", hint = "Run dotnet efstudio from this project directory." }));

app.Run();
