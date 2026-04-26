using System.Reflection;
using System.Runtime.Loader;

namespace EFStudio.Core.Isolation;

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
        if (ShouldUseDefaultContext(assemblyName))
        {
            return null;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath == null ? null : LoadFromAssemblyPath(assemblyPath);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath == null ? IntPtr.Zero : LoadUnmanagedDllFromPath(libraryPath);
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

        // Microsoft.Extensions.Configuration.* does not need to be shared for type identity —
        // EFStudio never accesses IConfigurationBuilder directly. Letting the target load its
        // own version avoids MissingMethodException when SetBasePath is unavailable in the
        // worker's runtime (e.g. rolled forward to .NET 10 where the method was removed).
        if (name.StartsWith("Microsoft.Extensions.Configuration", StringComparison.Ordinal))
        {
            return false;
        }

        return name.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal)
            || name.StartsWith("System.", StringComparison.Ordinal)
            || string.Equals(name, "System", StringComparison.Ordinal)
            || string.Equals(name, "mscorlib", StringComparison.Ordinal)
            || string.Equals(name, "netstandard", StringComparison.Ordinal);
    }
}
