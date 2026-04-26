using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using EFStudio.Contracts;
using EFStudio.Core.Isolation;

namespace EFStudio.Core.Services;

public sealed class DbContextCatalogLoader
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> ProjectBuildLocks = new(
        StringComparer.OrdinalIgnoreCase
    );

    public async Task<ITargetHost> LoadAsync(
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

        var targetContext = new TargetAssemblyContext(targetProject.TargetPath);

        try
        {
            var coreAssembly = targetContext.LoadFromAssemblyPath(Path.GetFullPath(typeof(DbContextCatalogLoader).Assembly.Location));
            var hostType = coreAssembly.GetType("EFStudio.Core.Isolation.TargetHost")
                ?? throw new InvalidOperationException("EFStudio could not resolve the isolated target host.");

            var host = Activator.CreateInstance(hostType, targetProject, startupProject) as ITargetHost;
            return host == null
                ? throw new InvalidOperationException("EFStudio could not create the isolated target host.")
                : new IsolatedTargetHost(targetContext, host);
        }
        catch
        {
            targetContext.Unload();
            throw;
        }
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

    private async Task<TargetProjectInfo> BuildProjectAsync(
        string projectPath,
        CancellationToken cancellationToken
    )
    {
        var normalizedProjectPath = Path.GetFullPath(projectPath);
        var buildLock = ProjectBuildLocks.GetOrAdd(
            normalizedProjectPath,
            static _ => new SemaphoreSlim(1, 1)
        );

        await buildLock.WaitAsync(cancellationToken);
        try
        {
            var frameworkInfo = await GetProjectPropertiesAsync(
                normalizedProjectPath,
                new[] { "TargetFramework", "TargetFrameworks" },
                framework: null,
                cancellationToken
            );

            var framework = SelectFramework(frameworkInfo);

            await RunDotNetAsync(
                new[]
                {
                    "build",
                    normalizedProjectPath,
                    "-nologo",
                    "-verbosity:minimal",
                    "-p:NuGetAudit=false",
                    "-p:TargetFramework=" + framework,
                },
                cancellationToken
            );

            var properties = await GetProjectPropertiesAsync(
                normalizedProjectPath,
                new[] { "TargetPath", "ProjectDir", "AssemblyName" },
                framework,
                cancellationToken
            );

            return new TargetProjectInfo(
                normalizedProjectPath,
                framework,
                GetRequiredProperty(properties, "TargetPath"),
                GetRequiredProperty(properties, "ProjectDir"),
                GetRequiredProperty(properties, "AssemblyName")
            );
        }
        finally
        {
            buildLock.Release();
        }
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

    private sealed class IsolatedTargetHost : ITargetHost
    {
        private readonly TargetAssemblyContext _loadContext;
        private readonly ITargetHost _innerHost;

        public IsolatedTargetHost(TargetAssemblyContext loadContext, ITargetHost innerHost)
        {
            _loadContext = loadContext;
            _innerHost = innerHost;
        }

        public IReadOnlyList<DbContextInfoContract> GetAvailableContexts() => _innerHost.GetAvailableContexts();

        public string? GetSelectedContextName() => _innerHost.GetSelectedContextName();

        public bool SelectContext(string contextName) => _innerHost.SelectContext(contextName);

        public Task<IReadOnlyList<TableInfoContract>> GetSchemaAsync(
            string? contextName,
            CancellationToken cancellationToken
        ) => _innerHost.GetSchemaAsync(contextName, cancellationToken);

        public Task<TablePageResponseContract?> GetTablePageAsync(
            string? contextName,
            TablePageRequestContract request,
            CancellationToken cancellationToken
        ) => _innerHost.GetTablePageAsync(contextName, request, cancellationToken);

        public Task<UpdateRecordsResponseContract> UpdateRecordsAsync(
            string? contextName,
            UpdateRecordsRequestContract request,
            CancellationToken cancellationToken
        ) => _innerHost.UpdateRecordsAsync(contextName, request, cancellationToken);

        public Task<DeleteRecordsResponseContract> DeleteRecordsAsync(
            string? contextName,
            DeleteRecordsRequestContract request,
            CancellationToken cancellationToken
        ) => _innerHost.DeleteRecordsAsync(contextName, request, cancellationToken);

        public void Dispose()
        {
            _innerHost.Dispose();
            _loadContext.Unload();
        }

        public async ValueTask DisposeAsync()
        {
            await _innerHost.DisposeAsync();
            _loadContext.Unload();
        }
    }
}
