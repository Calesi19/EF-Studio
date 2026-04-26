# Plan: EF Core Version Isolation via Plugin-Style AssemblyLoadContext

## Context

EFStudio bundles a specific EF Core patch version per TFM inside the worker (e.g., `EFCoreVersion=9.0.8` for net9.0). When a user's project references a newer patch (say 9.0.15), the worker loads EFStudio's older assembly into `AssemblyLoadContext.Default`, then tries to satisfy the user's 9.0.15 dependency by returning the already-loaded 9.0.8 — the runtime rejects this with `FileLoadException: 0x80131040` (manifest mismatch). All user types that reference EF Core types fail to load silently, resulting in "no DbContext found."

Bumping the pin version is a treadmill: every new EF Core patch from Microsoft breaks users on that patch until EFStudio publishes a new version. This is structurally unfixable without isolation.

## How dotnet ef solves this

`dotnet ef` uses `dotnet exec --depsfile [project.deps.json] ef.dll`. The key insight: it runs the design-time host **inside the project's own runtime** using the project's own `.deps.json`. EF Core is never loaded into the tool's own process at all — it lives only in the spawned `dotnet exec` process. This requires `Microsoft.EntityFrameworkCore.Design` as a per-project dev dependency.

EFStudio's worker subprocess approach is already similar in spirit (separate TFM per worker). The remaining gap is that EF Core still loads into the worker's `AssemblyLoadContext.Default`, meaning the worker version and the project version must agree on the patch.

## Per-project vs global tool

| Approach | Version compatibility | User experience | Maintenance |
|---|---|---|---|
| Per-project package (`dotnet add package EFStudio`) | Perfect — EF Core version comes from project | Requires adding a package to each project | No bundled EF Core needed |
| Global tool + isolated AssemblyLoadContext | Perfect — project's EF Core loads in isolation | Zero per-project setup; same UX as today | Slightly more complex host code |

**Recommendation: Keep the global tool model, add isolated AssemblyLoadContext.** Per-project install is a regression in UX for a tool designed to be developer-local. The isolation approach achieves the same version independence without requiring users to touch project files.

## Recommended Solution: Plugin-Style Isolated AssemblyLoadContext

### Architecture overview

```
EFStudio.Tool (net10.0) — no EF Core
    └─ spawns EFStudio.Worker (matching TFM) — no direct EF Core
           ├─ references EFStudio.Contracts (no EF Core)
           └─ creates TargetAssemblyContext (isolated, collectible)
                  ├─ loads target project assembly + EF Core from project's own deps
                  └─ runs ITargetHost implementation inside isolation
                         └─ returns only EFStudio.Contracts types back to Default context
```

The isolated `AssemblyLoadContext` loads the user's EF Core version. The Default context never sees EF Core at all. Communication is via a shared "contracts" assembly with no EF Core dependency.

### New project: EFStudio.Contracts

`EFStudio/EFStudio.Contracts/EFStudio.Contracts.csproj`

- Targets all supported TFMs (net6.0–net10.0)
- **No EF Core reference** — this is the only assembly shared between Default and isolated contexts
- Contains:
  - `ITargetHost` interface (replaces `IDbContextCatalog` at the cross-boundary layer)
  - DTOs that mirror current contracts but are EF-Core-free: `TargetContextInfo`, `TableSchema`, `TablePage`, `FieldValue`, `UpdateRequest`, `UpdateResult`
  - These are separate from the existing HTTP contracts in `EFStudio.Core/Contracts/` — they live at the isolation boundary

```csharp
// EFStudio.Contracts/ITargetHost.cs
public interface ITargetHost : IDisposable, IAsyncDisposable
{
    IReadOnlyList<TargetContextInfo> GetAvailableContexts();
    string? GetSelectedContextName();
    bool SelectContext(string contextName);
    Task<TableSchema> GetSchemaAsync(string? contextName, CancellationToken ct);
    Task<TablePage> GetTablePageAsync(string? contextName, TablePageQuery query, CancellationToken ct);
    Task<UpdateResult> UpdateRecordsAsync(string? contextName, UpdateRequest request, CancellationToken ct);
}
```

### Changes to EFStudio.Core

**New file: `EFStudio.Core/Isolation/TargetAssemblyContext.cs`**

Isolated, collectible `AssemblyLoadContext` that:
1. Is initialized with the target project's assembly path (resolved via `ProjectBuildInfo`)
2. Overrides `Load()` to:
   - Return `null` for `EFStudio.Contracts` — falls back to Default context so the shared interface types are identical
   - Use `AssemblyDependencyResolver` on the target project's path for everything else (EF Core, provider, user assemblies)
3. Is `isCollectible: true` so it can be unloaded when the tool exits

```csharp
internal sealed class TargetAssemblyContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public TargetAssemblyContext(string targetAssemblyPath)
        : base("EFStudioTarget", isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(targetAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Let EFStudio.Contracts resolve from Default — keeps interface types identical
        if (string.Equals(assemblyName.Name, "EFStudio.Contracts", StringComparison.OrdinalIgnoreCase))
            return null;

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}
```

**New file: `EFStudio.Core/Isolation/TargetHost.cs`**

Loaded inside the isolated context. Implements `ITargetHost`. Has full access to EF Core because EF Core loads inside the isolated context.

This class replaces the current activator/factory/lease pattern by internalizing all EF Core operations. It re-implements the logic currently spread across `DbContextCatalogLoader`, `SchemaService`, and `DataService` but at the isolation boundary.

```csharp
// Loaded inside TargetAssemblyContext — has EF Core
internal sealed class TargetHost : ITargetHost
{
    // Contains DbContext activators (factory or service-provider based)
    // Calls SchemaService and DataService internally
    // All return types are from EFStudio.Contracts — no EF Core types cross the boundary
}
```

**Modified: `EFStudio.Core/Services/DbContextCatalogLoader.cs`**

The `LoadAsync` method changes its output from `DiscoveredDbContextCatalog` to `ITargetHost`. The loading sequence becomes:

1. Build target project (unchanged — `BuildProjectAsync`)
2. Create `TargetAssemblyContext` from `ProjectBuildInfo.TargetPath`
3. Load `EFStudio.Core` assembly into the isolated context (needed because `TargetHost` is defined there but must run with the isolated EF Core)
4. Activate `TargetHost` via reflection through the isolated assembly reference
5. Cast result to `ITargetHost` — works because `EFStudio.Contracts` is shared from Default context

```csharp
// In LoadAsync — after building:
var targetContext = new TargetAssemblyContext(targetProject.TargetPath);
var coreAssembly = targetContext.LoadFromAssemblyPath(typeof(TargetHost).Assembly.Location);
var hostType = coreAssembly.GetType(typeof(TargetHost).FullName!)!;
var host = (ITargetHost)Activator.CreateInstance(hostType, targetProject, startupProject)!;
return host;
```

**Remove: `DbContextLease.cs`** — no longer needed; `ITargetHost` handles lifecycle internally.

**Modified: `IDbContextCatalog.cs`** — replace with `ITargetHost` or keep as a thin facade over `ITargetHost` in the Default context (StudioServer still needs a catalog-like interface; can adapt).

### Changes to EFStudio.Server

`StudioServer.cs` currently calls `catalog.LeaseDbContextAsync()` then passes `lease.Context` (a `DbContext`) to `SchemaService`/`DataService`. After the change:

- `StudioServer` depends on `ITargetHost` (from `EFStudio.Contracts`) instead of `IDbContextCatalog`
- Each endpoint calls `host.GetSchemaAsync(...)`, `host.GetTablePageAsync(...)`, `host.UpdateRecordsAsync(...)` directly
- `SchemaService` and `DataService` move inside `TargetHost` (or are kept as internal implementation details called from `TargetHost`) — they no longer need to be registered in the server's DI container
- Remove `IDbContextCatalog` registration from `StudioServer.StartAsync`; register `ITargetHost` instead

### Changes to EFStudio.Worker.csproj

Add reference to `EFStudio.Contracts`. Remove `EFStudio.Core` from direct dependencies if EFStudio.Core now bundles EF Core that would conflict — or keep it if EFStudio.Core is carefully built to only load EF Core in the isolated path.

> **Important:** EFStudio.Core itself still references EF Core at compile time (for `SchemaService`, `DataService`, `TargetHost`). The trick is that those types are **only instantiated inside `TargetAssemblyContext`**, never in Default context. The EFStudio.Core DLL that the worker loads into Default context must not be the EF-Core-referencing one — or EFStudio.Core must be split.

**Preferred split:** Extract `EFStudio.Contracts` out of `EFStudio.Core`, and ensure `EFStudio.Core` is only ever loaded inside the isolated context (not from the worker's Default context directly).

**Alternative (simpler):** Keep EFStudio.Core in Default context but ensure `TargetAssemblyContext.Load()` intercepts EFStudio.Core too and loads a second copy in isolation. This means EFStudio.Core's EF Core types live in isolation while the same DLL provides `ITargetHost` in the shared Default context — not possible (same assembly can't be in both). **The split is necessary.**

### Final project layout

```
EFStudio.Contracts/    — no EF Core; shared across Default and isolated contexts
EFStudio.Core/         — EF Core at compile time; only loaded inside TargetAssemblyContext
EFStudio.Server/       — references EFStudio.Contracts only (not EFStudio.Core, not EF Core)
EFStudio.Worker/       — references EFStudio.Contracts + EFStudio.Server; activates isolation
EFStudio.Tool/         — references EFStudio.Worker (unchanged)
```

### Critical files to modify

| File | Change |
|---|---|
| `EFStudio.Core/Services/DbContextCatalogLoader.cs` | Replace Default-context loading with `TargetAssemblyContext` creation; return `ITargetHost` |
| `EFStudio.Core/Services/IDbContextCatalog.cs` | Delete or repurpose; `ITargetHost` takes its role |
| `EFStudio.Core/Services/DbContextLease.cs` | Delete; lifecycle managed inside `TargetHost` |
| `EFStudio.Core/Services/SchemaService.cs` | Keep implementation; move instantiation to inside `TargetHost` |
| `EFStudio.Core/Services/DataService.cs` | Keep implementation; move instantiation to inside `TargetHost` |
| `EFStudio.Server/StudioServer.cs` | Accept `ITargetHost`; call host methods directly; remove `SchemaService`/`DataService` from DI |
| `EFStudio.Core/EFStudio.Core.csproj` | Keep EF Core refs; will only load inside isolation |
| `EFStudio.Server/EFStudio.Server.csproj` | Remove EFStudio.Core ref; add EFStudio.Contracts ref |
| `EFStudio.Worker/EFStudio.Worker.csproj` | Add EFStudio.Contracts ref; keep EFStudio.Core ref (for loading into isolation) |

**New files:**
- `EFStudio.Contracts/EFStudio.Contracts.csproj`
- `EFStudio.Contracts/ITargetHost.cs`
- `EFStudio.Contracts/TargetContextInfo.cs`, `TableSchema.cs`, `TablePage.cs`, `UpdateRequest.cs`, `UpdateResult.cs`, `TablePageQuery.cs`
- `EFStudio.Core/Isolation/TargetAssemblyContext.cs`
- `EFStudio.Core/Isolation/TargetHost.cs`

## Verification

1. Build: `dotnet build` from `EFStudio/` — all projects should compile
2. Pack: `dotnet pack EFStudio.Tool` — verify worker binaries and contracts DLL are included
3. Install locally: `dotnet tool install --global --add-source ./nupkg EFStudio`
4. Run against a net9.0 project with **any** EF Core 9.x patch: `efstudio --project /path/to/project.csproj --no-browser`
5. Verify schema loads: `curl http://localhost:<port>/efstudio/api/schema`
6. Run against a net8.0 project — verify isolation works across TFMs
7. Run `dotnet test EFStudio.Tests/` — existing tests should pass
8. Verify context unloads cleanly on exit (no finalizer warnings)
