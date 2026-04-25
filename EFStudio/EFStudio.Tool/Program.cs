using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using EFStudio.Core.Services;
using EFStudio.Server;

var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    var options = ToolOptions.Parse(args);
    if (options.ShowHelp)
    {
        ToolOptions.WriteHelp(Console.Out);
        return 0;
    }

    var loader = new DbContextCatalogLoader();
    await using var catalog = await loader.LoadAsync(
        new DbContextDiscoveryOptions(
            Environment.CurrentDirectory,
            options.ProjectPath,
            options.StartupProjectPath
        ),
        cancellationTokenSource.Token
    );

    if (!string.IsNullOrWhiteSpace(options.ContextName) && !catalog.SelectContext(options.ContextName))
    {
        Console.Error.WriteLine($"EFStudio could not find an available DbContext named '{options.ContextName}'.");
        return 1;
    }

    var availableContexts = catalog.GetAvailableContexts();
    if (availableContexts.Count == 0 || availableContexts.All(context => !context.IsAvailable))
    {
        foreach (var context in availableContexts)
        {
            if (!string.IsNullOrWhiteSpace(context.ActivationError))
            {
                Console.Error.WriteLine($"{context.Name}: {context.ActivationError}");
            }
        }

        return 1;
    }

    var port = options.Port ?? SelectPreferredPort(5123);
    var server = new StudioServer();
    await using var handle = await server.StartAsync(
        new StudioServerOptions($"http://127.0.0.1:{port}"),
        catalog,
        cancellationTokenSource.Token
    );

    Console.WriteLine($"EFStudio is running at {handle.StudioUri}");

    if (!options.NoBrowser)
    {
        TryOpenBrowser(handle.StudioUri);
    }

    await handle.WaitForShutdownAsync(cancellationTokenSource.Token);
    return 0;
}
catch (OperationCanceledException)
{
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(GetErrorMessage(exception));
    return 1;
}

static int SelectPreferredPort(int preferredPort)
{
    if (CanBind(preferredPort))
    {
        return preferredPort;
    }

    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static bool CanBind(int port)
{
    try
    {
        using var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
}

static void TryOpenBrowser(Uri uri)
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = uri.ToString(),
            UseShellExecute = true,
        });
    }
    catch
    {
        Console.WriteLine($"Open this URL in your browser: {uri}");
    }
}

static string GetErrorMessage(Exception exception)
{
    var effectiveException = exception is TargetInvocationException && exception.InnerException != null
        ? exception.InnerException
        : exception.GetBaseException();

    return effectiveException.Message;
}

internal sealed record ToolOptions(
    string? ProjectPath,
    string? StartupProjectPath,
    string? ContextName,
    int? Port,
    bool NoBrowser,
    bool ShowHelp
)
{
    public static ToolOptions Parse(string[] args)
    {
        string? projectPath = null;
        string? startupProjectPath = null;
        string? contextName = null;
        int? port = null;
        var noBrowser = false;
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
                case "--context":
                    contextName = ReadValue(args, ref index, argument);
                    break;
                case "--port":
                {
                    var value = ReadValue(args, ref index, argument);
                    if (!int.TryParse(value, out var parsedPort))
                    {
                        throw new InvalidOperationException($"EFStudio expected an integer after '{argument}'.");
                    }

                    port = parsedPort;
                    break;
                }
                case "--no-browser":
                    noBrowser = true;
                    break;
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                default:
                    throw new InvalidOperationException($"EFStudio does not recognize the option '{argument}'.");
            }
        }

        return new ToolOptions(projectPath, startupProjectPath, contextName, port, noBrowser, showHelp);
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
