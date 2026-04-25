using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using EFStudio.Core.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFStudio.Core.Services;

public sealed class DbContextCatalogLoader
{
    public async Task<DiscoveredDbContextCatalog> LoadAsync(
        DbContextDiscoveryOptions options,
        CancellationToken cancellationToken = default
    )
    {
        var targetProjectPath = ResolveProjectPath(options.ProjectPath, options.WorkingDirectory, "project");
        var startupProjectPath = ResolveProjectPath(
            options.StartupProjectPath,
            options.WorkingDirectory,
            "startup project",
            defaultPath: targetProjectPath
        );

        var targetProject = await BuildProjectAsync(targetProjectPath, cancellationToken);
        var startupProject = string.Equals(targetProjectPath, startupProjectPath, StringComparison.OrdinalIgnoreCase)
            ? targetProject
            : await BuildProjectAsync(startupProjectPath, cancellationToken);

        var loadContext = new StudioAssemblyLoadContext(startupProject.TargetPath);
        var startupAssembly = loadContext.LoadFromAssemblyPath(startupProject.TargetPath);
        var targetAssembly = string.Equals(startupProject.TargetPath, targetProject.TargetPath, StringComparison.OrdinalIgnoreCase)
            ? startupAssembly
            : loadContext.LoadFromAssemblyPath(targetProject.TargetPath);

        var startupHost = TryCreateStartupHost(startupAssembly, out var startupServices, out var startupError);
        var factoryTypes = GetLoadableTypes(startupAssembly)
            .Concat(GetLoadableTypes(targetAssembly))
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Distinct()
            .ToList();

        var contextTypes = GetLoadableTypes(targetAssembly)
            .Where(type => type is { IsAbstract: false } && IsDbContextType(type))
            .OrderBy(type => type.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (contextTypes.Count == 0)
        {
            loadContext.Unload();
            throw new InvalidOperationException(
                $"EFStudio could not find any DbContext types in '{targetProjectPath}'."
            );
        }

        var registrations = contextTypes
            .Select(contextType => CreateRegistration(contextType, factoryTypes, startupServices, startupError))
            .ToList();

        return new DiscoveredDbContextCatalog(loadContext, startupHost, registrations);
    }

    private static DbContextRegistration CreateRegistration(
        Type contextType,
        IReadOnlyList<Type> factoryTypes,
        IServiceProvider? startupServices,
        string? startupError
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
                DisplayName: contextType.FullName ?? contextName,
                CreatedByDesignTimeFactory: true,
                Activator: () =>
                {
                    var factory = Activator.CreateInstance(factoryType)
                        ?? throw new InvalidOperationException(
                            $"EFStudio could not create the design-time factory '{factoryType.FullName}'."
                        );

                    var createMethod = factoryType.GetMethod("CreateDbContext", new[] { typeof(string[]) })
                        ?? throw new InvalidOperationException(
                            $"EFStudio could not find CreateDbContext on '{factoryType.FullName}'."
                        );

                    var context = createMethod.Invoke(factory, new object[] { Array.Empty<string>() })
                        ?? throw new InvalidOperationException(
                            $"EFStudio could not create '{contextName}' through '{factoryType.FullName}'."
                        );

                    return new DbContextLease((DbContext)context);
                },
                ActivationError: null
            );
        }

        if (startupServices == null)
        {
            return new DbContextRegistration(
                contextName,
                DisplayName: contextType.FullName ?? contextName,
                CreatedByDesignTimeFactory: false,
                Activator: null,
                ActivationError:
                    startupError
                    ?? $"EFStudio could not create '{contextName}'. Add an IDesignTimeDbContextFactory<{contextName}> or expose a conventional startup builder method such as CreateHostBuilder."
            );
        }

        return new DbContextRegistration(
            contextName,
            DisplayName: contextType.FullName ?? contextName,
            CreatedByDesignTimeFactory: false,
            Activator: () =>
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
            ActivationError: null
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
                }
            }
            catch (Exception exception)
            {
                error =
                    $"EFStudio could not build the startup project's service provider from '{method.DeclaringType?.FullName}.{method.Name}': {exception.GetBaseException().Message}";
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
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).Cast<Type>().ToList();
        }
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

    private static string ResolveProjectPath(
        string? value,
        string workingDirectory,
        string description,
        string? defaultPath = null
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (!string.IsNullOrWhiteSpace(defaultPath))
            {
                return defaultPath;
            }

            return ResolveProjectPath(workingDirectory, workingDirectory, description, defaultPath: null);
        }

        var candidate = Path.IsPathRooted(value) ? value : Path.GetFullPath(Path.Combine(workingDirectory, value));

        if (Directory.Exists(candidate))
        {
            var projects = Directory.GetFiles(candidate, "*.csproj", SearchOption.TopDirectoryOnly);

            return projects.Length switch
            {
                1 => projects[0],
                0 => throw new InvalidOperationException(
                    $"EFStudio could not find a .csproj file in '{candidate}' for the {description}."
                ),
                _ => throw new InvalidOperationException(
                    $"EFStudio found multiple .csproj files in '{candidate}' for the {description}. Pass the specific project path with --project or --startup-project."
                ),
            };
        }

        if (File.Exists(candidate) && string.Equals(Path.GetExtension(candidate), ".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return candidate;
        }

        throw new InvalidOperationException(
            $"EFStudio could not find the {description} at '{candidate}'."
        );
    }

    private async Task<ProjectBuildInfo> BuildProjectAsync(
        string projectPath,
        CancellationToken cancellationToken
    )
    {
        var frameworkInfo = await GetProjectPropertiesAsync(
            projectPath,
            new[] { "TargetFramework", "TargetFrameworks" },
            framework: null,
            cancellationToken
        );

        var framework = SelectFramework(frameworkInfo);

        await RunDotNetAsync(
            new[]
            {
                "build",
                projectPath,
                "-nologo",
                "-verbosity:minimal",
                "-p:TargetFramework=" + framework,
            },
            cancellationToken
        );

        var properties = await GetProjectPropertiesAsync(
            projectPath,
            new[] { "TargetPath", "ProjectDir", "AssemblyName" },
            framework,
            cancellationToken
        );

        return new ProjectBuildInfo(
            projectPath,
            framework,
            GetRequiredProperty(properties, "TargetPath"),
            GetRequiredProperty(properties, "ProjectDir"),
            GetRequiredProperty(properties, "AssemblyName")
        );
    }

    private static string SelectFramework(JsonElement properties)
    {
        var targetFramework = GetOptionalProperty(properties, "TargetFramework");
        if (!string.IsNullOrWhiteSpace(targetFramework))
        {
            return targetFramework;
        }

        var frameworks = GetOptionalProperty(properties, "TargetFrameworks");
        if (!string.IsNullOrWhiteSpace(frameworks))
        {
            return frameworks
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .First();
        }

        throw new InvalidOperationException(
            "EFStudio could not determine the target framework for the selected project."
        );
    }

    private async Task<JsonElement> GetProjectPropertiesAsync(
        string projectPath,
        IReadOnlyList<string> propertyNames,
        string? framework,
        CancellationToken cancellationToken
    )
    {
        var arguments = new List<string> { "msbuild", projectPath };
        arguments.AddRange(propertyNames.Select(name => $"-getProperty:{name}"));

        if (!string.IsNullOrWhiteSpace(framework))
        {
            arguments.Add("-p:TargetFramework=" + framework);
        }

        var output = await RunDotNetAsync(arguments, cancellationToken);

        if (propertyNames.Count == 1)
        {
            var propertyName = propertyNames[0];
            var propertyValue = JsonSerializer.Serialize(output.Trim());
            using var singlePropertyDocument = JsonDocument.Parse(
                $"{{\"Properties\":{{\"{propertyName}\":{propertyValue}}}}}"
            );
            return singlePropertyDocument.RootElement.GetProperty("Properties").Clone();
        }

        using var document = JsonDocument.Parse(output);
        return document.RootElement.GetProperty("Properties").Clone();
    }

    private static string GetRequiredProperty(JsonElement properties, string name)
    {
        var value = GetOptionalProperty(properties, name);
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"EFStudio could not resolve the MSBuild property '{name}'."
            );
    }

    private static string? GetOptionalProperty(JsonElement properties, string name)
    {
        if (!properties.TryGetProperty(name, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Null ? null : property.GetString();
    }

    private static async Task<string> RunDotNetAsync(
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken
    )
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(cancellationToken);

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"EFStudio failed to run 'dotnet {string.Join(" ", arguments)}': {standardError.Trim()}"
            );
        }

        return string.IsNullOrWhiteSpace(standardOutput) ? standardError : standardOutput;
    }

    private sealed record ProjectBuildInfo(
        string ProjectPath,
        string Framework,
        string TargetPath,
        string ProjectDirectory,
        string AssemblyName
    );

    private sealed class StudioAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public StudioAssemblyLoadContext(string mainAssemblyPath)
            : base(nameof(StudioAssemblyLoadContext), isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var sharedAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly =>
                    string.Equals(assembly.GetName().Name, assemblyName.Name, StringComparison.Ordinal)
                );

            if (sharedAssembly != null)
            {
                return sharedAssembly;
            }

            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path == null ? null : LoadFromAssemblyPath(path);
        }
    }
}

public sealed class DiscoveredDbContextCatalog : IDbContextCatalog, IDisposable, IAsyncDisposable
{
    private readonly AssemblyLoadContext _loadContext;
    private readonly object? _startupHost;
    private readonly IReadOnlyList<DbContextRegistration> _registrations;
    private string? _selectedContextName;

    internal DiscoveredDbContextCatalog(
        AssemblyLoadContext loadContext,
        object? startupHost,
        IReadOnlyList<DbContextRegistration> registrations
    )
    {
        _loadContext = loadContext;
        _startupHost = startupHost;
        _registrations = registrations;

        var availableContexts = registrations.Where(registration => registration.ActivationError == null).ToList();
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

    public ValueTask<DbContextLease> LeaseDbContextAsync(
        string? contextName = null,
        CancellationToken cancellationToken = default
    )
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

        return ValueTask.FromResult(
            registration.Activator!()
        );
    }

    public void Dispose()
    {
        if (_startupHost is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _loadContext.Unload();
    }

    public async ValueTask DisposeAsync()
    {
        if (_startupHost is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (_startupHost is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _loadContext.Unload();
    }
}

internal sealed record DbContextRegistration(
    string Name,
    string DisplayName,
    bool CreatedByDesignTimeFactory,
    Func<DbContextLease>? Activator,
    string? ActivationError
);
