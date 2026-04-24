# EFStudio

**EFStudio** is a minimal, plug-and-play visual database editor for EF Core and PostgreSQL. Heavily inspired by Prisma Studio, it allows you to explore, edit, and manage your data directly from your browser with a single line of configuration in your ASP.NET Core application.

![EFStudio Screenshot](./docs/banner.webp)

## Features

- **Auto-Discovery**: Automatically maps your `DbContext` and PostgreSQL schemas.
- **Zero Config**: No separate installation or database connection strings required—it uses your existing EF Core setup.
- **CRUD Operations**: View, filter, create, update, and delete records through an intuitive interface.
- **Development-Only**: Designed as a middleware that only runs in the `Development` environment.

## Installation

Install the NuGet package via the .NET CLI:

```bash
dotnet add package EFStudio
```

## Quick Start

Enable EFStudio in your `Program.cs` file. Simply add the middleware within your development environment check:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseEFStudio();
}

app.Run();
```

By default, the studio will be available at `/efstudio` (e.g., `http://localhost:5000/efstudio`).

## Why EFStudio?

While tools like pgAdmin and Azure Data Studio are powerful, they often require external connections and context switching. **EFStudio** lives inside your project:

1. **Context Aware**: It understands your EF Core relations, navigations, and shadow properties.
2. **Minimal Footprint**: No need to manage credentials or connection strings in multiple places; if your API can connect to the DB, the Studio can too.
3. **Workflow Integration**: Keep your database management in the same lifecycle as your API development.

## Requirements

- .NET 6.0 or higher
- Entity Framework Core 6.0+
- Npgsql.EntityFrameworkCore.PostgreSQL

## Roadmap

- [ ] Support for complex many-to-many relationship editing.
- [ ] Raw SQL query console.
- [ ] Export data to CSV/JSON.
- [ ] Support for other providers (SQL Server, SQLite).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request or open an issue for any bugs or feature requests.

## License

MIT License - Copyright (c) 2026
