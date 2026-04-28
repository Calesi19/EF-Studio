# EFStudio Testing Plan

## Context

EFStudio has a well-started backend test suite (~30 tests across 7 files) but significant gaps in infrastructure, edge cases, and error paths. The frontend has **zero tests** and no test tooling at all. This plan covers every area that needs coverage and breaks work into staged deliverables.

---

## Current State

### Backend — What exists
| File | Coverage |
|------|----------|
| `DataServiceTests.cs` | Create, Read, Delete with type conversion |
| `MiddlewareTests.cs` | HTTP API endpoints (StudioServer) |
| `SchemaExplorerTests.cs` | Basic column extraction |
| `ToolDiscoveryTests.cs` | Sample project DbContext discovery |
| `ToolServerTests.cs` | End-to-end server startup |
| `PostgresIntegrationTests.cs` | 16+ PG types, multi-schema, FK constraints |
| `SampleSchemaConfigurationTests.cs` | Dynamic schema config via IConfiguration |

### Frontend — What exists
Nothing. No test runner, no test files, no test configuration.

---

## Exhaustive List of Things to Test

### Backend Gaps

**A. StudioAssetService** (`EFStudio.Core/StudioAssetService.cs`)
- `TryOpenAsset()` returns stream for known embedded manifest path
- `TryOpenAsset()` returns null for unknown path
- `TryOpenAsset()` falls back to `index.html` for paths with no file extension
- Content-type detection for `.js`, `.css`, `.ico`, `.png`, `.webp`, `.woff2`, `.svg`
- Default content-type for unrecognized extensions

**B. TableKeyFactory** (`EFStudio.Core/TableKeyFactory.cs`)
- `Create(schema, tableName)` with schema → `"schema.tableName"`
- `Create(null, tableName)` with no schema → `"tableName"`
- `Create(IEntityType)` reads schema from EF metadata

**C. EFStudioMiddleware** (`EFStudio.Core/EFStudioMiddleware.cs`)
- All CRUD routes routed correctly
- Asset serving from `/efstudio/*`
- `JsonException` on malformed body → 400
- Unknown route passes to next middleware
- Method validation (GET, POST, PUT, DELETE)

**D. ServiceCollectionExtensions** (`EFStudio.Core/ServiceCollectionExtensions.cs`)
- `AddEFStudio<TContext>()` registers all services
- `UseEFStudio()` adds middleware in development only
- `UseEFStudio(force: true)` adds middleware in any environment

**E. DataService edge cases** (`EFStudio.Core/DataService.cs`)
- `UpdateRecordsAsync`: modifies field values
- `UpdateRecordsAsync`: fails if PK column missing
- Type conversion: `Guid`, `DateOnly`, `TimeOnly`, `DateTimeOffset`
- Type conversion: `decimal`, `Enum` case-insensitive
- Type conversion: `byte[]` round-trip
- Nullable column: accepts `null`
- Non-nullable column: rejects `null`
- Shadow/navigation properties skipped during write
- `ApplyFilter` on non-string types
- `ApplySort` on numeric columns
- Pagination boundary: page beyond total pages
- Composite primary key CRUD

**F. SchemaService edge cases** (`EFStudio.Core/SchemaService.cs`)
- Navigation properties excluded from column list
- Owned entity types included
- Identity columns marked `IsIdentity = true`
- FK columns: `IsForeignKey = true`, `ForeignKeyTable` populated
- Schema-qualified table key generated

**G. TargetHost reflection** (`EFStudio.Core/Isolation/TargetHost.cs`)
- Discovers DbContext types, skips abstract/interface
- Uses `IDesignTimeDbContextFactory<T>` when available
- Attempts host builder factory methods in order
- Handles `ReflectionTypeLoadException` gracefully
- `SelectContext` returns false for unknown name
- `ActivationError` set when instantiation fails

**H. DbContextCatalogLoader** (`EFStudio.Core/DbContextCatalogLoader.cs`)
- Directory → single `.csproj` resolved
- Zero or multiple `.csproj` files → error
- `TargetFramework` extraction from MSBuild
- Multi-targeted `TargetFrameworks` → first chosen
- Build failure propagated
- Thread-safe concurrent builds

**I. Tool CLI** (`EFStudio.Tool/Program.cs`)
- `ToolOptions.Parse` with no args, `--project`, `--startup-project`, `--help`
- Unknown flags → error with usage hint
- Help text output format

**J. StudioServer error mapping** (`EFStudio.Server/StudioServer.cs`)
- `TargetHostException` propagates correct HTTP status
- `EFStudioRequestException` correct status + message
- `InvalidOperationException` → 400
- Unhandled → 500
- `TargetInvocationException` unwrapped

**K. Multi-schema (SQL Server)**
- Schema-prefixed table keys for SQL Server
- CRUD against schema-prefixed tables

---

### Frontend — All tests (greenfield)

**L. Test Infrastructure Setup**
- `vitest.config.ts`, `src/test/setup.ts`, MSW handlers, `renderWithProviders()`
- `package.json` test scripts

**M. Hooks**
- `useTableState`: filter, sort (string + numeric), pagination, `totalRows`/`totalPages`
- `useSettings`: read/write/default from localStorage

**N. API Layer**
- `fetchDbContexts`, `useDbContexts`, `useSelectDbContext`
- `fetchSchema`: `dataType` → `ColumnType` mapping, display name generation
- `fetchTableData`: URL params, null normalization, object stringification
- `createRecords`, `updateRecords`, `deleteRecords`: payload shapes

**O. Pure Logic**
- `normalizeColumnType` for all known type strings
- `buildCreateDraftRow`: per-type defaults
- `serializeRowPk`: composite PK string format
- `generateDisplayName`: PascalCase → spaced

**P. UI Components — Unit**
- `ColumnTypeBadge`: label per type
- `DataTablePagination`: page info, prev/next disabled states
- `DataTableToolbar`: calls `onFilterChange`
- `DataTableSkeleton`: renders skeletons
- `RecordFormField`: string/number/boolean/datetime/FK/readonly inputs
- `DeleteConfirmDialog`: confirm and cancel callbacks

**Q. UI Components — Interaction**
- `DataTableHeader`: sort toggle behavior
- `DataTableRow`: checkbox selection
- `DataTableCell`: edit mode, value change, FK link click
- `CreateRecordDrawer`: form submission
- `FKPickerDrawer`: row selection

**R. Context / Integration**
- `selectTable`: new vs existing tab
- `changeSort`/`changeFilter`: resets pagination
- `changePage`, `changePageSize`: localStorage persistence
- `setCellEdit`: pending edits accumulation
- `saveEdits`: PUT mutation, clears edits
- `discardEdits`: clears pending state
- `deleteRows`: DELETE mutation, selection reset
- `submitCreateRow`: POST mutation, query invalidation
- `jumpToReference`: tab with pre-filled filter
- `selectContext`: clears tabs, POST mutation, query invalidation

**S. End-to-End / Smoke (Playwright)**
- Context selection → table list visible
- Open table → rows in grid
- Edit cell → save → value persists
- Create row → appears in grid
- Delete row → disappears
- Sort column → order changes
- Filter → matches visible

---

## Implementation Stages

- [x] **Stage 1** — Frontend Test Infrastructure (`vitest`, MSW, `renderWithProviders`) — **145 frontend tests total**
- [x] **Stage 2** — Frontend: Hooks & Pure Logic (`useTableState`, `useSettings`, utils)
- [x] **Stage 3** — Frontend: API Layer (all API functions + React Query hooks)
- [x] **Stage 4** — Frontend: Component Unit Tests (all UI components)
- [x] **Stage 5** — Frontend: Context Integration Tests (`StudioContext` full state machine)
- [x] **Stage 6** — Backend: Unit Tests for Untested Services (`StudioAssetService`, `ServiceCollectionExtensions`, `TableKeyFactory`) — **67 backend tests total**
- [x] **Stage 7** — Backend: DataService Edge Cases (Update, pagination, filter, sort)
- [x] **Stage 8** — Backend: Tool CLI & StudioServer Error Mapping (`ToolOptions`, error status codes)
- [ ] **Stage 9** — Backend: TargetHost & DbContextCatalogLoader (reflection, build pipeline)
- [ ] **Stage 10** — End-to-End: Playwright smoke tests (optional)

---

## Critical Files

| File | Stage |
|------|-------|
| `EFStudio.App/vitest.config.ts` | 1 |
| `EFStudio.App/src/test/setup.ts` | 1 |
| `EFStudio.App/src/test/handlers/` | 1 |
| `EFStudio.App/src/hooks/__tests__/useTableState.test.ts` | 2 |
| `EFStudio.App/src/api/**/__tests__/*.test.ts` | 3 |
| `EFStudio.App/src/components/**/__tests__/*.test.tsx` | 4 |
| `EFStudio.App/src/pages/StudioPage/context/__tests__/StudioContext.test.tsx` | 5 |
| `EFStudio.Tests/CoreTests/StudioAssetServiceTests.cs` | 6 |
| `EFStudio.Tests/CoreTests/EFStudioMiddlewareTests.cs` | 6 |
| `EFStudio.Tests/CoreTests/DataServiceTests.cs` (extend) | 7 |
| `EFStudio.Tests/CoreTests/ToolOptionsTests.cs` | 8 |
| `EFStudio.Tests/CoreTests/TargetHostTests.cs` | 9 |
