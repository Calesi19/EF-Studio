# Contributing to EFStudio

Thanks for contributing to EFStudio.

## Before You Start

- Open an issue for bugs, feature requests, or larger design changes when you want early feedback.
- Keep changes focused. Smaller pull requests are easier to review and safer to merge.
- Do not include unrelated refactors in the same PR.

## Project Layout

- `EFStudio/EFStudio.Core/` contains the .NET library and middleware code.
- `EFStudio/EFStudio.Core/client/` contains the React frontend served by the library.
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

### Frontend

From `EFStudio/EFStudio.Core/client/`:

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
