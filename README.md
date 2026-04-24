# EFStudio

**EFStudio** is a minimal, plug-and-play visual database browser for EF Core apps. Heavily inspired by Prisma Studio, it lets you inspect schema and records directly from your browser with a single line of configuration in your ASP.NET Core application.

![EFStudio Screenshot](./docs/banner.webp)

## Features

- **Auto-Discovery**: Automatically maps your `DbContext`, EF Core entities, and PostgreSQL schemas.
- **Zero Config**: No separate installation or database connection strings required. It uses your existing EF Core setup.
- **Read-Only Browsing**: View and filter records through an embedded interface without enabling writes.
- **Development-Only**: Designed as a middleware that only runs in the `Development` environment.

## Installation

Install the NuGet package via the .NET CLI:

```bash
dotnet add package EFStudio
```

## Quick Start

Enable EFStudio in your `Program.cs` file by registering it with your EF Core `DbContext`, then adding the middleware to the pipeline:

```csharp
using EFStudio.Core.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddEFStudio<AppDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseEFStudio();
}

app.Run();
```

Note: the NuGet package name is `EFStudio`, while the current extension-method namespace is `EFStudio.Core.Extensions`.

By default, the studio will be available at `/efstudio` (e.g., `http://localhost:5000/efstudio`).

## Why EFStudio?

While tools like pgAdmin and Azure Data Studio are powerful, they often require external connections and context switching. **EFStudio** lives inside your project:

1. **Context Aware**: It understands your EF Core relations, navigations, and mapped schemas.
2. **Minimal Footprint**: No need to manage credentials or connection strings in multiple places; if your API can connect to the DB, the Studio can too.
3. **Workflow Integration**: Keep database inspection in the same lifecycle as your API development.

## Requirements

- .NET 6.0 or higher
- Entity Framework Core 6.0+
- EF Core provider for your database
- `Npgsql.EntityFrameworkCore.PostgreSQL` for PostgreSQL
- `Microsoft.EntityFrameworkCore.Sqlite` for SQLite

## Roadmap

- [ ] Support for complex many-to-many relationship editing.
- [ ] Raw SQL query console.
- [ ] Export data to CSV/JSON.
- [ ] Support for write operations.
- [ ] Support for other providers (SQL Server).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request or open an issue for any bugs or feature requests.

## License

MIT License - Copyright (c) 2026
