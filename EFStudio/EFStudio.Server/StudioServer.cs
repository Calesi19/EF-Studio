using System.Text.Json;
using EFStudio.Core.Contracts;
using EFStudio.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EFStudio.Server;

public sealed class StudioServer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<StudioServerHandle> StartAsync(
        StudioServerOptions options,
        IDbContextCatalog dbContextCatalog,
        CancellationToken cancellationToken = default
    )
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(options.Url);
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(console =>
        {
            console.SingleLine = true;
            console.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddSingleton(dbContextCatalog);
        builder.Services.AddSingleton<ISchemaService, SchemaService>();
        builder.Services.AddSingleton<IDataService, DataService>();
        builder.Services.AddSingleton<StudioAssetService>();

        var app = builder.Build();

        app.MapGet("/", () => Results.Redirect("/efstudio"));
        app.MapGet("/efstudio/api/health", () => Results.Ok(new { status = "ok", readOnly = true }));

        app.MapGet("/efstudio/api/contexts", (IDbContextCatalog catalog) =>
            Results.Json(
                new DbContextListResponseContract(
                    catalog.GetAvailableContexts(),
                    catalog.GetSelectedContextName()
                ),
                JsonOptions
            )
        );

        app.MapPost(
            "/efstudio/api/contexts/select",
            async (HttpContext httpContext, IDbContextCatalog catalog) =>
            {
                var request = await httpContext.Request.ReadFromJsonAsync<SelectDbContextRequestContract>(
                    JsonOptions,
                    httpContext.RequestAborted
                );

                if (request == null || !catalog.SelectContext(request.ContextName))
                {
                    return Results.NotFound(
                        new ErrorResponseContract(
                            $"The DbContext '{request?.ContextName}' could not be selected."
                        )
                    );
                }

                return Results.Json(
                    new DbContextListResponseContract(
                        catalog.GetAvailableContexts(),
                        catalog.GetSelectedContextName()
                    ),
                    JsonOptions
                );
            }
        );

        app.MapGet(
            "/efstudio/api/schema",
            async (
                HttpContext httpContext,
                IDbContextCatalog catalog,
                ISchemaService schemaService
            ) =>
            {
                try
                {
                    var contextName = httpContext.Request.Query["context"].ToString();
                    await using var lease = await catalog.LeaseDbContextAsync(
                        string.IsNullOrWhiteSpace(contextName) ? null : contextName,
                        httpContext.RequestAborted
                    );

                    return Results.Json(schemaService.GetSchema(lease.Context), JsonOptions);
                }
                catch (Exception exception)
                {
                    return Results.BadRequest(new ErrorResponseContract(exception.Message));
                }
            }
        );

        app.MapGet(
            "/efstudio/api/tables",
            async (
                HttpContext httpContext,
                IDbContextCatalog catalog,
                ISchemaService schemaService
            ) =>
            {
                try
                {
                    var contextName = httpContext.Request.Query["context"].ToString();
                    await using var lease = await catalog.LeaseDbContextAsync(
                        string.IsNullOrWhiteSpace(contextName) ? null : contextName,
                        httpContext.RequestAborted
                    );

                    return Results.Json(schemaService.GetSchema(lease.Context), JsonOptions);
                }
                catch (Exception exception)
                {
                    return Results.BadRequest(new ErrorResponseContract(exception.Message));
                }
            }
        );

        app.MapGet(
            "/efstudio/api/data",
            async (HttpContext httpContext, IDbContextCatalog catalog, IDataService dataService) =>
            {
                var tableKey = httpContext.Request.Query["table"].ToString();
                if (string.IsNullOrWhiteSpace(tableKey))
                {
                    return Results.BadRequest(
                        new ErrorResponseContract("Choose a table before requesting table data.")
                    );
                }

                try
                {
                    var contextName = httpContext.Request.Query["context"].ToString();
                    await using var lease = await catalog.LeaseDbContextAsync(
                        string.IsNullOrWhiteSpace(contextName) ? null : contextName,
                        httpContext.RequestAborted
                    );

                    var page = await dataService.GetTablePageAsync(
                        lease.Context,
                        new TablePageRequestContract(
                            tableKey,
                            ParseInt(httpContext.Request.Query["page"], 1),
                            ParseInt(httpContext.Request.Query["pageSize"], 50),
                            httpContext.Request.Query["filter"],
                            httpContext.Request.Query["sortColumn"],
                            httpContext.Request.Query["sortDirection"]
                        ),
                        httpContext.RequestAborted
                    );

                    return page == null
                        ? Results.NotFound(
                            new ErrorResponseContract($"The table '{tableKey}' could not be found.")
                        )
                        : Results.Json(page, JsonOptions);
                }
                catch (Exception exception)
                {
                    return Results.BadRequest(new ErrorResponseContract(exception.Message));
                }
            }
        );

        app.MapGet(
            "/efstudio/{**assetPath}",
            async (
                HttpContext httpContext,
                string? assetPath,
                StudioAssetService assetService,
                ILogger<StudioServer> logger
            ) =>
            {
                if (!assetService.TryOpenAsset(assetPath, out var stream, out var contentType) || stream == null)
                {
                    logger.LogWarning("EFStudio asset not found for path {Path}.", assetPath);
                    return Results.NotFound();
                }

                return Results.Stream(stream, contentType);
            }
        );

        await app.StartAsync(cancellationToken);
        return new StudioServerHandle(app, options.StudioUri);
    }

    private static int ParseInt(string? value, int fallback)
    {
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
