using EFStudio.Core.Extensions;
using EFStudio.Sample.Sqlite.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
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
