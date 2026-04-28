using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using EFStudio.Contracts;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace EFStudio.Core.Isolation;

internal sealed class TargetHost : ITargetHost
{
    private readonly object? _startupHost;
    private readonly IReadOnlyList<DbContextRegistration> _registrations;
    private readonly SchemaService _schemaService = new(NullLogger<SchemaService>.Instance);
    private readonly DataService _dataService = new(NullLogger<DataService>.Instance);
    private string? _selectedContextName;

    public TargetHost(TargetProjectInfo targetProject, TargetProjectInfo startupProject)
    {
        var startupAssembly = LoadAssembly(startupProject.TargetPath);
        var targetAssembly = string.Equals(startupProject.TargetPath, targetProject.TargetPath, StringComparison.OrdinalIgnoreCase)
            ? startupAssembly
            : LoadAssembly(targetProject.TargetPath);

        _startupHost = TryCreateStartupHost(
            startupAssembly,
            out var startupServices,
            out var startupError
        );

        var factoryTypes = GetLoadableTypes(startupAssembly)
            .Concat(GetLoadableTypes(targetAssembly))
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Distinct()
            .ToList();

        var (targetTypes, targetLoaderExceptions) = GetLoadableTypesWithDiagnostics(targetAssembly);
        var contextTypes = targetTypes
            .Where(type => type is { IsAbstract: false } && IsDbContextType(type))
            .OrderBy(type => type.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (contextTypes.Count == 0)
        {
            throw CreateNoContextException(targetProject.ProjectPath, targetTypes.Count, targetLoaderExceptions);
        }

        _registrations = contextTypes
            .Select(contextType => CreateRegistration(contextType, factoryTypes, startupServices, startupError, startupProject.ProjectDirectory))
            .ToList();

        var availableContexts = _registrations.Where(registration => registration.ActivationError == null).ToList();
        _selectedContextName = availableContexts.Count == 1 ? availableContexts[0].Name : null;
    }

    public IReadOnlyList<DbContextInfoContract> GetAvailableContexts()
    {
        return _registrations
            .Select(registration => new DbContextInfoContract(
                registration.Name,
                registration.DisplayName,
                registration.Name == _selectedContextName,
                _registrations.Count == 1 && registration.ActivationError == null,
                registration.ActivationError == null,
                registration.CreatedByDesignTimeFactory,
                registration.ActivationError
            ))
            .ToList();
    }

    public string? GetSelectedContextName() => _selectedContextName;

    public bool SelectContext(string contextName)
    {
        var registration = _registrations.FirstOrDefault(item =>
            string.Equals(item.Name, contextName, StringComparison.Ordinal)
        );

        if (registration == null || registration.ActivationError != null)
        {
            return false;
        }

        _selectedContextName = registration.Name;
        return true;
    }

    public async Task<IReadOnlyList<TableInfoContract>> GetSchemaAsync(
        string? contextName,
        CancellationToken cancellationToken
    )
    {
        await using var lease = LeaseDbContext(contextName, cancellationToken);
        return _schemaService.GetSchema(lease.Context);
    }

    public async Task<TablePageResponseContract?> GetTablePageAsync(
        string? contextName,
        TablePageRequestContract request,
        CancellationToken cancellationToken
    )
    {
        await using var lease = LeaseDbContext(contextName, cancellationToken);
        return await _dataService.GetTablePageAsync(lease.Context, request, cancellationToken);
    }

    public async Task<CreateRecordsResponseContract> CreateRecordsAsync(
        string? contextName,
        CreateRecordsRequestContract request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var lease = LeaseDbContext(contextName, cancellationToken);
            return await _dataService.CreateRecordsAsync(lease.Context, request, cancellationToken);
        }
        catch (EFStudioRequestException exception)
        {
            throw new TargetHostException(exception.StatusCode, exception.Message, exception);
        }
    }

    public async Task<UpdateRecordsResponseContract> UpdateRecordsAsync(
        string? contextName,
        UpdateRecordsRequestContract request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var lease = LeaseDbContext(contextName, cancellationToken);
            return await _dataService.UpdateRecordsAsync(lease.Context, request, cancellationToken);
        }
        catch (EFStudioRequestException exception)
        {
            throw new TargetHostException(exception.StatusCode, exception.Message, exception);
        }
    }

    public async Task<DeleteRecordsResponseContract> DeleteRecordsAsync(
        string? contextName,
        DeleteRecordsRequestContract request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var lease = LeaseDbContext(contextName, cancellationToken);
            return await _dataService.DeleteRecordsAsync(lease.Context, request, cancellationToken);
        }
        catch (EFStudioRequestException exception)
        {
            throw new TargetHostException(exception.StatusCode, exception.Message, exception);
        }
    }

    public void Dispose()
    {
        switch (_startupHost)
        {
            case IDisposable disposable:
                disposable.Dispose();
                break;
            case IAsyncDisposable asyncDisposable:
                asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                break;
        }
    }

    public async ValueTask DisposeAsync()
    {
        switch (_startupHost)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    private DbContextLease LeaseDbContext(string? contextName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var effectiveContextName = contextName ?? _selectedContextName;
        if (string.IsNullOrWhiteSpace(effectiveContextName))
        {
            throw new InvalidOperationException(
                "EFStudio found multiple DbContexts. Select one with --context or from the UI before loading schema data."
            );
        }

        var registration = _registrations.FirstOrDefault(item =>
            string.Equals(item.Name, effectiveContextName, StringComparison.Ordinal)
        );

        if (registration == null)
        {
            throw new InvalidOperationException(
                $"EFStudio could not find a DbContext named '{effectiveContextName}'."
            );
        }

        if (registration.ActivationError != null)
        {
            throw new InvalidOperationException(registration.ActivationError);
        }

        return registration.Activator!();
    }

    private static DbContextRegistration CreateRegistration(
        Type contextType,
        IReadOnlyList<Type> factoryTypes,
        IServiceProvider? startupServices,
        string? startupError,
        string projectDirectory
    )
    {
        var contextName = contextType.Name;
        var factoryType = factoryTypes.FirstOrDefault(type =>
            type.GetInterfaces().Any(@interface => IsDesignTimeFactoryFor(@interface, contextType))
        );

        if (factoryType != null)
        {
            return new DbContextRegistration(
                contextName,
                contextType.FullName ?? contextName,
                true,
                () =>
                {
                    var factory = Activator.CreateInstance(factoryType)
                        ?? throw new InvalidOperationException(
                            $"EFStudio could not create the design-time factory '{factoryType.FullName}'."
                        );

                    var createMethod = factoryType.GetMethod("CreateDbContext", new[] { typeof(string[]) })
                        ?? throw new InvalidOperationException(
                            $"EFStudio could not find CreateDbContext on '{factoryType.FullName}'."
                        );

                    // Set working directory to the project directory so that design-time
                    // factories using Directory.GetCurrentDirectory() to find appsettings.json
                    // resolve correctly, matching EF Core CLI tool behavior.
                    var previousDirectory = Directory.GetCurrentDirectory();
                    try
                    {
                        Directory.SetCurrentDirectory(projectDirectory);
                        var context = createMethod.Invoke(factory, new object[] { Array.Empty<string>() })
                            ?? throw new InvalidOperationException(
                                $"EFStudio could not create '{contextName}' through '{factoryType.FullName}'."
                            );
                        return new DbContextLease((DbContext)context);
                    }
                    finally
                    {
                        Directory.SetCurrentDirectory(previousDirectory);
                    }
                },
                null
            );
        }

        if (startupServices == null)
        {
            return new DbContextRegistration(
                contextName,
                contextType.FullName ?? contextName,
                false,
                null,
                startupError
                ?? $"EFStudio could not create '{contextName}'. Add an IDesignTimeDbContextFactory<{contextName}> or expose a conventional startup builder method such as CreateHostBuilder."
            );
        }

        return new DbContextRegistration(
            contextName,
            contextType.FullName ?? contextName,
            false,
            () =>
            {
                var scopeFactory = startupServices.GetRequiredService<IServiceScopeFactory>();
                var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetService(contextType) as DbContext;

                if (context == null)
                {
                    scope.Dispose();
                    throw new InvalidOperationException(
                        $"EFStudio could not resolve '{contextName}' from the startup project's service provider. Add an IDesignTimeDbContextFactory<{contextName}> or register the DbContext in the startup project."
                    );
                }

                return new DbContextLease(context, scope);
            },
            null
        );
    }

    private static object? TryCreateStartupHost(
        Assembly startupAssembly,
        out IServiceProvider? services,
        out string? error
    )
    {
        services = null;
        error = null;

        var candidateMethods = GetFactoryMethods(startupAssembly).ToList();
        if (candidateMethods.Count == 0)
        {
            error =
                "EFStudio could not find a conventional startup builder method on the startup assembly. Add an IDesignTimeDbContextFactory<TContext> or expose CreateHostBuilder/CreateWebHostBuilder.";
            return null;
        }

        foreach (var method in candidateMethods)
        {
            try
            {
                var result = method.Invoke(null, new object[] { Array.Empty<string>() });
                switch (result)
                {
                    case IHostBuilder hostBuilder:
                    {
                        var host = hostBuilder.Build();
                        services = host.Services;
                        return host;
                    }
#pragma warning disable ASPDEPR008
                    case IWebHostBuilder webHostBuilder:
                    {
                        var webHost = webHostBuilder.Build();
                        services = webHost.Services;
                        return webHost;
                    }
                    case IHost host:
                        services = host.Services;
                        return host;
                    case IWebHost webHost:
                        services = webHost.Services;
                        return webHost;
#pragma warning restore ASPDEPR008
                }
            }
            catch (Exception exception)
            {
                error =
                    $"EFStudio could not build the startup project's service provider from '{method.DeclaringType?.FullName}.{method.Name}': {GetEffectiveException(exception).Message}";
            }
        }

        error ??=
            "EFStudio could not create the startup project's service provider. Add an IDesignTimeDbContextFactory<TContext> or provide a conventional CreateHostBuilder/CreateWebHostBuilder method.";
        return null;
    }

    private static IEnumerable<MethodInfo> GetFactoryMethods(Assembly assembly)
    {
        var types = GetLoadableTypes(assembly)
            .OrderByDescending(type => type == assembly.EntryPoint?.DeclaringType)
            .ToList();

        foreach (var type in types)
        {
            foreach (var methodName in new[] { "CreateHostBuilder", "CreateWebHostBuilder", "BuildWebHost" })
            {
                var method = type.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(string[]) },
                    modifiers: null
                );

                if (method != null)
                {
                    yield return method;
                }
            }
        }
    }

    private static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
    {
        return GetLoadableTypesWithDiagnostics(assembly).Types;
    }

    private static (IReadOnlyList<Type> Types, IReadOnlyList<Exception> LoaderExceptions) GetLoadableTypesWithDiagnostics(Assembly assembly)
    {
        try
        {
            return (assembly.GetTypes(), Array.Empty<Exception>());
        }
        catch (ReflectionTypeLoadException exception)
        {
            var types = exception.Types.Where(type => type != null).Cast<Type>().ToList();
            var loaderExceptions = exception.LoaderExceptions.Where(e => e != null).Cast<Exception>().ToList();
            return (types, loaderExceptions);
        }
    }

    private static Assembly LoadAssembly(string assemblyPath)
    {
        return AssemblyLoadContext.GetLoadContext(typeof(TargetHost).Assembly)!
            .LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
    }

    private static bool IsDbContextType(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            if (string.Equals(current.FullName, typeof(DbContext).FullName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDesignTimeFactoryFor(Type interfaceType, Type contextType)
    {
        return interfaceType.IsGenericType
            && string.Equals(
                interfaceType.GetGenericTypeDefinition().FullName,
                typeof(IDesignTimeDbContextFactory<>).FullName,
                StringComparison.Ordinal
            )
            && string.Equals(
                interfaceType.GenericTypeArguments[0].FullName,
                contextType.FullName,
                StringComparison.Ordinal
            );
    }

    private static InvalidOperationException CreateNoContextException(
        string targetProjectPath,
        int targetTypeCount,
        IReadOnlyList<Exception> targetLoaderExceptions
    )
    {
        var builder = new StringBuilder();
        builder.Append($"EFStudio could not find any DbContext types in '{targetProjectPath}'.");

        if (targetLoaderExceptions.Count > 0)
        {
            builder.Append(" Some types failed to load. Loader errors:");
            foreach (var loaderException in targetLoaderExceptions.Take(5))
            {
                builder.Append($" [{loaderException.GetType().Name}: {loaderException.Message}]");
            }

            if (targetLoaderExceptions.Count > 5)
            {
                builder.Append($" ... and {targetLoaderExceptions.Count - 5} more.");
            }
        }
        else if (targetTypeCount == 0)
        {
            builder.Append(" The assembly appears to contain no loadable types. Verify the project builds successfully with 'dotnet build'.");
        }
        else
        {
            builder.Append($" Found {targetTypeCount} type(s) in the assembly but none inherit from DbContext.");
        }

        return new InvalidOperationException(builder.ToString());
    }

    private static Exception GetEffectiveException(Exception exception)
    {
        return exception is TargetInvocationException { InnerException: not null }
            ? exception.InnerException
            : exception.GetBaseException();
    }

    private sealed record DbContextRegistration(
        string Name,
        string DisplayName,
        bool CreatedByDesignTimeFactory,
        Func<DbContextLease>? Activator,
        string? ActivationError
    );
}
