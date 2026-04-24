using System.Text.Json;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFStudio.Core.Middleware;

public class EFStudioMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _rootPath = "/efstudio";

    public EFStudioMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        SchemaExplorer explorer,
        IServiceProvider serviceProvider
    )
    {
        var dbContext = serviceProvider.GetRequiredService<DbContext>();
        var path = context.Request.Path.Value ?? "";

        if (path.Equals($"{_rootPath}/api/schema", StringComparison.OrdinalIgnoreCase))
        {
            var schema = explorer.GetSchema(dbContext);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(schema));
            return;
        }

        if (path.Equals($"{_rootPath}/api/data", StringComparison.OrdinalIgnoreCase))
        {
            var tableName = context.Request.Query["table"].ToString();
            if (string.IsNullOrEmpty(tableName))
            {
                context.Response.StatusCode = 400;
                return;
            }

            var dataService = serviceProvider.GetRequiredService<DataService>();
            var data = await dataService.GetTableDataAsync(dbContext, tableName);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(data));
            return;
        }

        if (path.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            await ServeEmbeddedFile(context, path);
            return;
        }

        await _next(context);
    }

    private async Task ServeEmbeddedFile(HttpContext context, string path)
    {
        var assembly = typeof(EFStudioMiddleware).Assembly;

        string resourcePath =
            path.Length <= _rootPath.Length ? "index.html" : path.Substring(_rootPath.Length + 1);
        if (string.IsNullOrEmpty(resourcePath))
            resourcePath = "index.html";

        var manifestPath = $"EFStudio.Core.wwwroot.{resourcePath.Replace("/", ".")}";

        using var stream = assembly.GetManifestResourceStream(manifestPath);

        if (stream == null)
        {
            Console.WriteLine($"[EFStudio] Resource not found: {manifestPath}");
            context.Response.StatusCode = 404;
            return;
        }

        context.Response.ContentType = GetContentType(resourcePath);
        await stream.CopyToAsync(context.Response.Body);
    }

    private string GetContentType(string path) =>
        path.EndsWith(".js") ? "application/javascript"
        : path.EndsWith(".css") ? "text/css"
        : "text/html";
}
