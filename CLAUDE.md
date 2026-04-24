# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

EFStudio is a Prisma Studio-style visual database editor for .NET apps. It plugs into an ASP.NET Core app as a middleware (`app.UseEFStudio()`) and auto-discovers the app's `DbContext` to expose a browser UI for exploring and editing EF Core/PostgreSQL data. It is development-only by design.

The repo has two parts:
- **`EFStudio/EFStudio.Core/`** — the .NET 9 class library that will become the NuGet package (middleware, API, DbContext introspection)
- **`EFStudio/EFStudio.Core/client/`** — the React frontend that gets embedded in the package and served at `/efstudio`
- **`EFStudio/EFStudio.Sample/`** — a bare ASP.NET Core Web API used for manual integration testing

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

`EFStudio.Core` is currently a stub. The intended architecture is:
- A middleware class that serves the embedded React `dist/` at `/efstudio`
- API endpoints (e.g. `/efstudio/api/tables`, `/efstudio/api/tables/{name}/records`) that the frontend will call
- A `DbContextInspector` service that uses EF Core metadata APIs (`IModel`, `IEntityType`) to discover tables, columns, relationships, and types — replacing the mock data in the frontend
- `app.UseEFStudio()` extension method on `IApplicationBuilder` as the single integration point

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
