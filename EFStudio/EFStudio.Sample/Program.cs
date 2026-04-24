using EFStudio.Core.Extensions;
using EFStudio.Sample.Sqlite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ReplaceService<IModelCacheKeyFactory, SampleModelCacheKeyFactory>()
);

builder.Services.AddEFStudio<AppDbContext>();

var app = builder.Build();

// 3. Auto-create database for the demo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    await SampleDataSeeder.SeedAsync(db);
}

// 4. Enable EFStudio UI at /efstudio during development
app.UseEFStudio();

app.MapGet("/", () => Results.Redirect("/efstudio"));

app.Run();
