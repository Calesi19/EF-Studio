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
