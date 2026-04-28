using System.Reflection;
using System.Runtime.Loader;

namespace EFStudio.Core.Isolation;

internal sealed class TargetAssemblyContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _targetResolver;
    private readonly AssemblyDependencyResolver? _workerResolver;

    public TargetAssemblyContext(string targetAssemblyPath)
        : base("EFStudioTarget", isCollectible: true)
    {
        _targetResolver = new AssemblyDependencyResolver(targetAssemblyPath);

        var entryLocation = Assembly.GetEntryAssembly()?.Location;
        _workerResolver = entryLocation != null ? new AssemblyDependencyResolver(entryLocation) : null;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (ShouldUseDefaultContext(assemblyName))
        {
            return null;
        }

        // Prefer the target project's own deps. For Microsoft.Extensions.Configuration.*
        // assemblies not found there (e.g. FileExtensions lives in the shared framework, not
        // NuGet), fall back to the worker's resolver before the default context. Without this,
        // FileConfigurationExtensions lands in the default context while IConfigurationBuilder
        // was loaded from the target's NuGet deps — two type identities — causing
        // MissingMethodException on SetBasePath and similar calls.
        var assemblyPath = _targetResolver.ResolveAssemblyToPath(assemblyName)
            ?? ResolveConfigurationFromWorker(assemblyName);

        return assemblyPath == null ? null : LoadFromAssemblyPath(assemblyPath);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _targetResolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath == null ? IntPtr.Zero : LoadUnmanagedDllFromPath(libraryPath);
    }

    private string? ResolveConfigurationFromWorker(AssemblyName assemblyName)
    {
        if (_workerResolver == null)
        {
            return null;
        }

        if (assemblyName.Name?.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal) != true)
        {
            return null;
        }

        var path = _workerResolver.ResolveAssemblyToPath(assemblyName);
        if (path != null)
        {
            return path;
        }

        // Shared-framework Microsoft.Extensions.* assemblies (e.g. FileProviders, Configuration.FileExtensions)
        // are not listed in any deps.json so neither resolver can find them by path. The default
        // context has them loaded from the runtime directory — use that same physical path
        // here so that when the assembly resolves its interfaces it goes through
        // this context's Load(), picking up the isolated NuGet copies and keeping type
        // identities consistent.
        return AssemblyLoadContext.Default.Assemblies
            .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            ?.Location;
    }

    private static bool ShouldUseDefaultContext(AssemblyName assemblyName)
    {
        var name = assemblyName.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return true;
        }

        if (string.Equals(name, "EFStudio.Contracts", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // System.* must be shared — the CLR primitive types cannot have two identities.
        // Microsoft.AspNetCore.* and Microsoft.Extensions.* are intentionally NOT excluded here:
        // the resolver tries the target project's deps first (handles version mismatches, e.g.
        // a net9.0 project referencing Extensions 10.x from NuGet), and returns null if it
        // can't resolve, which falls back to the default context as before.
        return name.StartsWith("System.", StringComparison.Ordinal)
            || string.Equals(name, "System", StringComparison.Ordinal)
            || string.Equals(name, "mscorlib", StringComparison.Ordinal)
            || string.Equals(name, "netstandard", StringComparison.Ordinal);
    }
}
