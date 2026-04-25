# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

EFStudio is a Prisma Studio-style visual database editor for .NET apps. It is delivered as a .NET global tool that builds the target project, discovers available `DbContext` types, starts a local server, and opens a browser UI for exploring EF Core data. It is development-only by design.

The repo has these main parts:
- **`EFStudio/EFStudio.Tool/`** — the .NET global tool entry point (`dotnet efstudio`)
- **`EFStudio/EFStudio.Server/`** — the local web server that exposes the EFStudio API and serves the embedded frontend
- **`EFStudio/EFStudio.Core/`** — shared contracts, schema/data services, DbContext discovery, and embedded assets
- **`EFStudio/EFStudio.Core/client/`** — the React frontend embedded into the server
- **`EFStudio/EFStudio.Sample/`** — a sample ASP.NET Core app used for manual tool testing
- **`EFStudio/EFStudio.Tests/`** — automated tests for discovery, schema, data, and server behavior

## Frontend Commands

All commands run from `EFStudio/EFStudio.Core/client/`:

```bash
npm run dev        # start dev server (Vite HMR)
npm run build      # tsc -b && vite build (output to dist/)
npm run lint       # eslint
npm run preview    # preview production build
```

TypeScript is checked as part of `build`. To type-check without building:
```bash
npx tsc -p tsconfig.app.json --noEmit
```
> Note: TypeScript 6 emits a deprecation warning for `baseUrl` in tsconfig — this is expected and non-blocking.

To add a new Shadcn component:
```bash
npx shadcn@latest add <component-name>
```

## .NET Commands

From `EFStudio/` (solution root):

```bash
dotnet build                        # build solution
dotnet run --project EFStudio.Sample  # run sample app
dotnet run --project EFStudio.Tool -- --project ./EFStudio.Sample/EFStudio.Sample.csproj
dotnet test EFStudio.Tests/EFStudio.Tests.csproj
```

## Frontend Architecture

The frontend is a single-page React 19 app with no routing. All state lives in `App.tsx` and is passed down as props — no context, no external state library.

## Frontend Guidelines

- Use TanStack React Query for server data fetching and caching.
- Keep every API function in its own file, and keep that function's React Query hook in the same file.
- Prefer page-level Context providers for page state, fetched data composition, and data manipulation.
- Break overloaded frontend files into smaller focused modules when a file starts owning multiple concerns.
- Avoid unexplained numeric literals. Extract them into descriptive named constants.
- Exceptions to the magic-number rule: `0`, `1`, and `-1` when used idiomatically.

### API Pattern

Each API file should contain both the request function and the React Query hook for that request.

```ts
import { useQuery } from "@tanstack/react-query";
import axiosInstance from "../../helpers/axiosInstance";

export async function fetchProject(projectId: string): Promise<Project> {
  const response = await axiosInstance.get(`/projects/${projectId}`);
  return response.data.result;
}

export function useProject(projectId: string, enabled = true) {
  return useQuery({
    queryKey: ["project", projectId],
    queryFn: () => fetchProject(projectId),
    enabled: enabled && !!projectId,
  });
}
```

### Page State Pattern

Use React Context API for page-level state, data fetching, and data manipulation. Each page or area should own its own context.

```text
src/pages/
└── FeaturePage/
    ├── index.tsx
    ├── context/
    │   └── FeatureContext.tsx
    └── components/
        └── FeatureContent.tsx
```

```tsx
import { createContext, useContext, type ReactNode } from "react";
import { useFeatureData } from "../../../api/feature/fetchFeatureData";

type FeatureContextType = {
  data: FeatureData[];
  isLoading: boolean;
};

const FeatureContext = createContext<FeatureContextType | undefined>(undefined);

export function FeatureContextProvider({ children }: { children: ReactNode }) {
  const { data, isLoading } = useFeatureData();

  return (
    <FeatureContext.Provider value={{ data: data ?? [], isLoading }}>
      {children}
    </FeatureContext.Provider>
  );
}

export function useFeatureContext() {
  const context = useContext(FeatureContext);
  if (!context) throw new Error("useFeatureContext must be used within provider");
  return context;
}
```

**Data flow:**
1. `MOCK_TABLES` in `src/data/mock.ts` defines the shape and seed data for 4 tables (User, Post, Tag, PostTag)
2. `App.tsx` holds a mutable `Map<string, RecordRow[]>` — edits update this map directly
3. `useTableState` hook (`src/hooks/useTableState.ts`) derives `paginatedRows / totalRows / totalPages` from the raw rows + current filter/sort/pagination state
4. Selecting a table resets filter, sort, and pagination

**Key type contracts** (`src/types/index.ts`):
- `ColumnDef` — describes a column (name, type, isPrimaryKey, isForeignKey, isNullable, foreignKeyTable)
- `TableDef` — a table's columns + rows
- `RecordRow = Record<string, FieldValue>` — a single row; always check for `null` before calling string methods on values

**CRUD pattern:**
- Create/Edit → `RecordDialog` (mode prop: `"create" | "edit"`) → `RecordForm` → `RecordFormField` (renders control by `ColumnDef.type`)
- Delete → `DeleteConfirmDialog` (AlertDialog)
- FK columns render as `<Select>` populated from the referenced table's rows

**Shadcn setup:** style `radix-maia`, color `mist`, icon library `hugeicons` (`@hugeicons/react`). CSS variables use oklch. Dark mode uses the `.dark` class. Sidebar tokens (`--sidebar`, `--sidebar-accent`, etc.) are already defined in `index.css`.

When adding components with icon buttons, wrap the app with `TooltipProvider` — it is already present in `App.tsx`.

## .NET Architecture

The current backend architecture is:
- `EFStudio.Tool` parses CLI options, loads the target/startup project, discovers `DbContext` types, and starts the local studio server
- `DbContextCatalogLoader` builds the target project and creates a catalog of available `DbContext` activators, using either design-time factories or the startup service provider
- `StudioServer` hosts the local HTTP endpoints under `/efstudio`, serves the embedded frontend, and uses the selected `DbContext` on demand
- `SchemaService` and `DataService` use EF Core metadata and runtime access to return schema information and paged table data
- The sample project supports discovery through `IDesignTimeDbContextFactory<AppDbContext>`; application middleware wiring is no longer the integration model

## Backend Guidelines

### Do

- Keep logic one-dimensional.
- Use `record` types for DTOs and models.
- Use dependency injection.
- Follow the Single Responsibility Principle.
- Always define request and response contracts in their own `*Contract.cs` files.
- Return user-friendly error messages that can be shown directly in the frontend.
- Include logging for important operations, failures, and debugging via `ILogger<T>`.
- Prefer a functional style where practical: LINQ, projections, immutable data, and expressions over statement-heavy code.

### Do Not

- Do not use recursion.
