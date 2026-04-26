<p align="center">
  <img src="./docs/logo.webp" alt="EFStudio logo" width="180" />
</p>

<p align="center">
  Visual database browsing for EF Core apps.
</p>

<p align="center">
  <a href="https://github.com/Calesi19/EFStudio/releases">
    <img src="https://img.shields.io/github/v/release/Calesi19/EFStudio" alt="GitHub Release" />
  </a>
  <a href="https://github.com/Calesi19/EFStudio/actions/workflows/publish-nuget.yml">
    <img src="https://github.com/Calesi19/EFStudio/actions/workflows/publish-nuget.yml/badge.svg" alt="Publish NuGet Package" />
  </a>
  <a href="https://github.com/Calesi19/EFStudio/actions/workflows/pipeline.yml">
    <img src="https://github.com/Calesi19/EFStudio/actions/workflows/pipeline.yml/badge.svg" alt="Build and Test" />
  </a>
</p>

**EFStudio** is a local browser-based workbench for EF Core apps. Install it as a .NET global tool, run `dotnet efstudio` inside your project directory, and it discovers your `DbContext` types without modifying your application startup.

![EFStudio Demo](./docs/demo.webp)

## Features

- **Auto-Discovery**: Finds your target project, builds it, discovers `DbContext` types, and supports multiple contexts.
- **Zero App Integration**: No package registration or middleware wiring is required.
- **Read-Only Browsing**: View schema and paged records through a local embedded UI.
- **Local-Only Server**: Hosts the workbench on `localhost` and opens it in your browser.

## Installation

Install the .NET tool via the .NET CLI:

```bash
dotnet tool install --global EFStudio
```

## Quick Start

Run EFStudio from inside the project that contains your EF Core setup:

```bash
dotnet efstudio
```

EFStudio builds the project, discovers available `DbContext` types, starts a local server, and opens the studio in your browser at `/efstudio`.

Optional arguments:

```bash
dotnet efstudio --project ./SomeProject.csproj
dotnet efstudio --startup-project ./SomeApi.csproj
dotnet efstudio --context AppDbContext
dotnet efstudio --port 5123
dotnet efstudio --no-browser
```

EFStudio prefers `IDesignTimeDbContextFactory<TContext>` when creating a context. If no design-time factory exists, it will try to create the startup project's service provider by using a conventional startup builder method such as `CreateHostBuilder`. If neither path works, EFStudio returns a clear error telling you to add `IDesignTimeDbContextFactory<TContext>` or provide more startup configuration.

## Typical Workflow

1. Open a terminal in the project directory that contains your EF Core app.
2. Run `dotnet efstudio`.
3. Let EFStudio discover your available `DbContext` types.
4. Use the browser UI to inspect schema and browse records locally.
5. Stop the tool when you are done.

## Local Development

Build the solution from the solution root:

```bash
cd EFStudio
dotnet build
```

Run the sample API:

```bash
dotnet run --project EFStudio.Sample
```

Run the tool against the sample project:

```bash
dotnet run --project EFStudio.Tool -- --project ./EFStudio.Sample/EFStudio.Sample.csproj
```

Pack and install the tool locally for testing:

```bash
dotnet pack EFStudio.Tool -c Release
dotnet tool install --global --add-source ./EFStudio.Tool/bin/Release EFStudio
```

## Why EFStudio?

1. **Context Aware**: It understands your EF Core relations, navigations, and mapped schemas.
2. **Minimal Footprint**: No need to manage credentials or connection strings in multiple places, and no need to rely on a separate database workbench. If your API can connect to the database, the Studio can too.
3. **Workflow Integration**: Keep database inspection in the same lifecycle as your API development.

## Requirements

- .NET SDK 10.0 or higher to install and run the global tool
- Entity Framework Core 6.0+
- A buildable .NET project that can create your target `DbContext`
- An EF Core provider for your database
- `Npgsql.EntityFrameworkCore.PostgreSQL` for PostgreSQL
- `Microsoft.EntityFrameworkCore.Sqlite` for SQLite

## Database Support

| Database        | Status                  |
| --------------- | ----------------------- |
| PostgreSQL      | ✅ Supported            |
| SQLite          | ✅ Supported            |
| SQL Server      | 🚧 Coming in the future |
| MySQL           | 🚧 Coming in the future |
| MariaDB         | 🚧 Coming in the future |
| Oracle Database | 🚧 Coming in the future |

## Contributing

See [CONTRIBUTE.md](./CONTRIBUTE.md) for local setup, development workflow, and pull request guidance.

## License

MIT License - Copyright (c) 2026
