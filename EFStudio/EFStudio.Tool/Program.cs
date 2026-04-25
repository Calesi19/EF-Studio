using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;

try
{
    var options = ToolOptions.Parse(args);
    if (options.ShowHelp)
    {
        ToolOptions.WriteHelp(Console.Out);
        return 0;
    }

    var projectPath = ResolveProjectPath(options.ProjectPath, Environment.CurrentDirectory, "project");
    var startupProjectPath = ResolveProjectPath(
        options.StartupProjectPath,
        Environment.CurrentDirectory,
        "startup project",
        defaultPath: projectPath
    );

    var startupFramework = await GetSelectedTargetFrameworkAsync(startupProjectPath);
    var workerFramework = GetSupportedWorkerFramework(startupFramework);
    var workerDllPath = GetWorkerDllPath(workerFramework);

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            WorkingDirectory = Environment.CurrentDirectory,
        },
    };

    process.StartInfo.ArgumentList.Add(workerDllPath);
    foreach (var argument in args)
    {
        process.StartInfo.ArgumentList.Add(argument);
    }

    process.Start();
    await process.WaitForExitAsync();
    return process.ExitCode;
}
catch (Exception exception)
{
    Console.Error.WriteLine(GetErrorMessage(exception));
    return 1;
}

static string ResolveProjectPath(
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

static async Task<string> GetSelectedTargetFrameworkAsync(string projectPath)
{
    var output = await RunDotNetAsync(
        new[]
        {
            "msbuild",
            projectPath,
            "-getProperty:TargetFramework",
            "-getProperty:TargetFrameworks",
        }
    );

    using var document = JsonDocument.Parse(output);
    var properties = document.RootElement.GetProperty("Properties");

    var framework = GetProperty(properties, "TargetFramework");
    if (!string.IsNullOrWhiteSpace(framework))
    {
        return framework;
    }

    var frameworks = GetProperty(properties, "TargetFrameworks");
    if (!string.IsNullOrWhiteSpace(frameworks))
    {
        return frameworks
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .First();
    }

    throw new InvalidOperationException(
        $"EFStudio could not determine the target framework for '{projectPath}'."
    );
}

static string? GetProperty(JsonElement properties, string name)
{
    return properties.TryGetProperty(name, out var property) && property.ValueKind != JsonValueKind.Null
        ? property.GetString()
        : null;
}

static string GetSupportedWorkerFramework(string targetFramework)
{
    var match = Regex.Match(targetFramework, "^net(?<major>\\d+)\\.(?<minor>\\d+)$");
    if (!match.Success || !int.TryParse(match.Groups["major"].Value, out var majorVersion))
    {
        throw new InvalidOperationException(
            $"EFStudio supports .NET projects targeting net6.0 through net10.0. Found '{targetFramework}'."
        );
    }

    var workerFramework = $"net{majorVersion}.0";
    return workerFramework switch
    {
        "net6.0" or "net7.0" or "net8.0" or "net9.0" or "net10.0" => workerFramework,
        _ => throw new InvalidOperationException(
            $"EFStudio supports .NET projects targeting net6.0 through net10.0. Found '{targetFramework}'."
        ),
    };
}

static string GetWorkerDllPath(string workerFramework)
{
    var workerDllPath = Path.Combine(
        AppContext.BaseDirectory,
        "workers",
        workerFramework,
        "EFStudio.Worker.dll"
    );

    return File.Exists(workerDllPath)
        ? workerDllPath
        : throw new InvalidOperationException(
            $"EFStudio could not find the worker runtime for '{workerFramework}' at '{workerDllPath}'."
        );
}

static async Task<string> RunDotNetAsync(IReadOnlyList<string> arguments)
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
    await process.WaitForExitAsync();

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

static string GetErrorMessage(Exception exception)
{
    return exception.GetBaseException().Message;
}

internal sealed record ToolOptions(
    string? ProjectPath,
    string? StartupProjectPath,
    bool ShowHelp
)
{
    public static ToolOptions Parse(string[] args)
    {
        string? projectPath = null;
        string? startupProjectPath = null;
        var showHelp = false;

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--project":
                    projectPath = ReadValue(args, ref index, argument);
                    break;
                case "--startup-project":
                    startupProjectPath = ReadValue(args, ref index, argument);
                    break;
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
            }
        }

        return new ToolOptions(projectPath, startupProjectPath, showHelp);
    }

    public static void WriteHelp(TextWriter writer)
    {
        writer.WriteLine("Usage: dotnet efstudio [options]");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  --project <path>           Target project that contains the DbContext types.");
        writer.WriteLine("  --startup-project <path>   Startup project used to build the service provider.");
        writer.WriteLine("  --context <name>           DbContext name to preselect.");
        writer.WriteLine("  --port <port>              Local port for the EFStudio server.");
        writer.WriteLine("  --no-browser               Do not open the browser automatically.");
        writer.WriteLine("  --help                     Show command help.");
    }

    private static string ReadValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new InvalidOperationException($"EFStudio expected a value after '{optionName}'.");
        }

        index++;
        return args[index];
    }
}
