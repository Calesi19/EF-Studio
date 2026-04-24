using System.Text.Json;
using EFStudio.Core.Contracts;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFStudio.Core.Middleware;

public class EFStudioMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EFStudioMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private const string RootPath = "/efstudio";
    private const string SchemaPath = $"{RootPath}/api/schema";
    private const string DataPath = $"{RootPath}/api/data";

    public EFStudioMiddleware(RequestDelegate next, ILogger<EFStudioMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        SchemaService schemaService,
        DataService dataService,
        DbContext dbContext
    )
    {
        var path = context.Request.Path.Value ?? "";

        try
        {
            if (path.Equals(SchemaPath, StringComparison.OrdinalIgnoreCase))
            {
                var schema = schemaService.GetSchema(dbContext);
                await WriteJsonAsync(context, StatusCodes.Status200OK, schema);
                return;
            }

            if (path.Equals(DataPath, StringComparison.OrdinalIgnoreCase))
            {
                var tableName = context.Request.Query["table"].ToString();
                if (string.IsNullOrEmpty(tableName))
                {
                    await WriteErrorAsync(
                        context,
                        StatusCodes.Status400BadRequest,
                        "Choose a table before requesting table data."
                    );
                    return;
                }

                var request = new TableDataRequestContract(tableName);
                var data = await dataService.GetTableDataAsync(
                    dbContext,
                    request,
                    context.RequestAborted
                );

                if (data == null)
                {
                    await WriteErrorAsync(
                        context,
                        StatusCodes.Status404NotFound,
                        $"The table '{tableName}' could not be found."
                    );
                    return;
                }

                await WriteJsonAsync(context, StatusCodes.Status200OK, data);
                return;
            }

            if (path.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
            {
                await ServeEmbeddedFile(context, path);
                return;
            }

            await _next(context);
        }
        catch (Exception exception) when (
            path.Equals(SchemaPath, StringComparison.OrdinalIgnoreCase) ||
            path.Equals(DataPath, StringComparison.OrdinalIgnoreCase)
        )
        {
            _logger.LogError(exception, "EFStudio request failed for path {Path}.", path);
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "EFStudio could not complete that request. Try again in a moment."
            );
        }
    }

    private async Task ServeEmbeddedFile(HttpContext context, string path)
    {
        var assembly = typeof(EFStudioMiddleware).Assembly;

        string resourcePath =
            path.Length <= RootPath.Length ? "index.html" : path.Substring(RootPath.Length + 1);
        if (string.IsNullOrEmpty(resourcePath))
            resourcePath = "index.html";

        var manifestPath = $"EFStudio.Core.wwwroot.{resourcePath.Replace("/", ".")}";

        using var stream = assembly.GetManifestResourceStream(manifestPath);

        if (stream == null)
        {
            _logger.LogWarning("EFStudio embedded asset not found: {ResourcePath}.", manifestPath);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = GetContentType(resourcePath);
        await stream.CopyToAsync(context.Response.Body);
    }

    private static Task WriteJsonAsync(HttpContext context, int statusCode, object payload)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        _logger.LogWarning("EFStudio request failed with status {StatusCode}: {Message}", statusCode, message);
        return WriteJsonAsync(context, statusCode, new ErrorResponseContract(message));
    }

    private string GetContentType(string path) =>
        path.EndsWith(".js") ? "application/javascript"
        : path.EndsWith(".css") ? "text/css"
        : "text/html";
}
