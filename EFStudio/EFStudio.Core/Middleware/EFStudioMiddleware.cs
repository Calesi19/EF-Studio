using System.Text.Json;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Core.Middleware;

public class EFStudioMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _rootPath = "/efstudio";

    public EFStudioMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, DbContext dbContext, SchemaExplorer explorer)
    {
        var path = context.Request.Path.Value ?? "";

        // 1. Handle API Request for Schema
        if (path.Equals($"{_rootPath}/api/schema", StringComparison.OrdinalIgnoreCase))
        {
            var schema = explorer.GetSchema(dbContext);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(schema));
            return;
        }

        // 2. Handle React Static Files (Embedded)
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

        // Default to index.html for the root or deep-links (SPA routing)
        string resourcePath =
            path.Length <= _rootPath.Length ? "index.html" : path.Substring(_rootPath.Length + 1);
        if (string.IsNullOrEmpty(resourcePath))
            resourcePath = "index.html";

        // Map path to your manifest resource name (Namespace.Folder.File)
        var manifestPath = $"EFStudio.Core.wwwroot.{resourcePath.Replace("/", ".")}";

        using var stream = assembly.GetManifestResourceStream(manifestPath);
        if (stream == null)
        {
            // Fallback to index.html for SPA support
            using var indexStream = assembly.GetManifestResourceStream(
                "EFStudio.Core.wwwroot.index.html"
            );
            await indexStream!.CopyToAsync(context.Response.Body);
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
