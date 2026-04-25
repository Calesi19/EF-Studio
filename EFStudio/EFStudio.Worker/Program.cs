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
        new StudioServerOptions($"http://localhost:{port}"),
        catalog,
        cancellationTokenSource.Token
    );

    PrintStartupBanner(handle.BaseUri, handle.StudioUri, options.NoBrowser);

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

    var listener = new TcpListener(IPAddress.Loopback, 0);
    try
    {
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
    finally
    {
        listener.Stop();
    }
}

static bool CanBind(int port)
{
    var listener = new TcpListener(IPAddress.Loopback, port);
    try
    {
        listener.Start();
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
    finally
    {
        listener.Stop();
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
    }
}

static void PrintStartupBanner(Uri baseUri, Uri studioUri, bool noBrowser)
{
    var lines = new[]
    {
        "EFStudio is ready",
        $"Host: {baseUri}",
        $"UI:   {studioUri}",
        noBrowser ? "Browser: disabled (--no-browser)" : "Browser: opening automatically",
    };

    var contentWidth = lines.Max(static line => line.Length);
    var border = $"+-{new string('-', contentWidth)}-+";

    WriteBannerLine(border, ConsoleColor.DarkCyan);

    for (var index = 0; index < lines.Length; index++)
    {
        var line = lines[index];
        if (index == 0)
        {
            WriteBannerContent(line, contentWidth, ConsoleColor.Green);
            continue;
        }

        var labelWidth = line.IndexOf(':');
        if (labelWidth <= 0)
        {
            WriteBannerContent(line, contentWidth, ConsoleColor.Gray);
            continue;
        }

        WriteBannerKeyValueContent(
            line[..labelWidth],
            line[(labelWidth + 1)..].TrimStart(),
            contentWidth,
            ConsoleColor.Cyan,
            ConsoleColor.White
        );
    }

    WriteBannerLine(border, ConsoleColor.DarkCyan);
}

static void WriteBannerLine(string text, ConsoleColor color)
{
    var previousColor = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ForegroundColor = previousColor;
}

static void WriteBannerContent(string text, int contentWidth, ConsoleColor color)
{
    var previousColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("| ");
    Console.ForegroundColor = color;
    Console.Write(text.PadRight(contentWidth));
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine(" |");
    Console.ForegroundColor = previousColor;
}

static void WriteBannerKeyValueContent(
    string label,
    string value,
    int contentWidth,
    ConsoleColor labelColor,
    ConsoleColor valueColor)
{
    var previousColor = Console.ForegroundColor;
    var content = $"{label}: {value}";
    var padding = new string(' ', contentWidth - content.Length);

    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write("| ");
    Console.ForegroundColor = labelColor;
    Console.Write(label);
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.Write(": ");
    Console.ForegroundColor = valueColor;
    Console.Write(value);
    Console.Write(padding);
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine(" |");
    Console.ForegroundColor = previousColor;
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
