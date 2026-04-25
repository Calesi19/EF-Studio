# Contributing to EFStudio

Thanks for contributing to EFStudio.

## Before You Start

- Open an issue for bugs, feature requests, or larger design changes when you want early feedback.
- Keep changes focused. Smaller pull requests are easier to review and safer to merge.
- Do not include unrelated refactors in the same PR.

## Project Layout

- `EFStudio/EFStudio.Core/` contains the .NET library and middleware code.
- `EFStudio/EFStudio.App/` contains the React frontend served by the library.
- `EFStudio/EFStudio.Sample/` is the sample ASP.NET Core app for manual testing.
- `docs/site/` contains the documentation site source.

## Local Setup

### .NET

From `EFStudio/`:

```bash
dotnet restore
dotnet build
```

Run the sample app:

```bash
dotnet run --project EFStudio.Sample
```

### Tool Development

The primary development workflow is now the CLI tool plus the sample project.

From `EFStudio/`, run EFStudio against the sample project:

```bash
dotnet run --project EFStudio.Tool -- --project ./EFStudio.Sample/EFStudio.Sample.csproj
```

Useful variations:

```bash
dotnet run --project EFStudio.Tool -- --project ./EFStudio.Sample/EFStudio.Sample.csproj --no-browser
dotnet run --project EFStudio.Tool -- --project ./EFStudio.Sample/EFStudio.Sample.csproj --port 5123
dotnet run --project EFStudio.Tool -- --project ./EFStudio.Sample/EFStudio.Sample.csproj --context AppDbContext
```

Typical local development loop:

1. Start the sample API if you need to validate the sample project itself:

```bash
dotnet run --project EFStudio.Sample
```

2. In a separate terminal, start the local EFStudio tool host:

```bash
dotnet run --project EFStudio.Tool -- --project ./EFStudio.Sample/EFStudio.Sample.csproj
```

3. Open the local EFStudio URL printed by the tool if `--no-browser` was used.

If you want to test the packaged tool shape instead of `dotnet run`, build and invoke the tool assembly directly:

```bash
dotnet build EFStudio.Tool/EFStudio.Tool.csproj
dotnet EFStudio.Tool/bin/Debug/net10.0/EFStudio.Tool.dll --project ./EFStudio.Sample/EFStudio.Sample.csproj --no-browser
```

The sample project includes `IDesignTimeDbContextFactory<AppDbContext>` so the tool can discover and create the context without requiring `app.UseEFStudio()`.

### Frontend

From `EFStudio/EFStudio.App/`:

```bash
npm install
npm run dev
```

Useful commands:

```bash
npm run build
npm run lint
```

## Contribution Workflow

1. Create a branch for your change.
2. Make the smallest change that solves the problem cleanly.
3. Add or update tests when behavior changes.
4. Run the relevant build, lint, and test commands before opening a PR.
5. Update documentation when the user-facing behavior or setup changes.

## Running Tests

From `EFStudio/`, run the full .NET test suite with:

```bash
dotnet test EFStudio.Tests/EFStudio.Tests.csproj
```

If you only want to verify the test project compiles, use:

```bash
dotnet test EFStudio.Tests/EFStudio.Tests.csproj --no-restore -m:1 -nr:false
```

Useful targeted runs:

```bash
dotnet test EFStudio.Tests/EFStudio.Tests.csproj --filter FullyQualifiedName~ToolServerTests
dotnet test EFStudio.Tests/EFStudio.Tests.csproj --filter FullyQualifiedName~DataServiceTests
dotnet test EFStudio.Tests/EFStudio.Tests.csproj --filter FullyQualifiedName~PostgresIntegrationTests
```

Notes:

- Most tests run against in-memory SQLite or test-only contexts and do not need any manual setup.
- `PostgresIntegrationTests` use `Testcontainers.PostgreSql`, so Docker must be installed and running.
- The test project also includes model-configuration coverage for the sample `AppDbContext`.
- If you changed the global tool or server startup flow, run the full test suite before opening a PR.

## Pull Requests

- Describe the problem being solved and the approach you took.
- Call out any tradeoffs, limitations, or follow-up work.
- Include screenshots or recordings for UI changes.
- Link the related issue when applicable.

## Testing Expectations

- Backend changes should pass `dotnet build` and any relevant tests.
- Frontend changes should pass `npm run build` and `npm run lint`.
- Changes that affect the embedded UI should be checked in `EFStudio.Sample` when possible.

## Documentation

Update these files when relevant:

- `README.md` for project-level usage and positioning.
- `docs/site/` for documentation site content.
- Code comments only when they clarify non-obvious behavior.
