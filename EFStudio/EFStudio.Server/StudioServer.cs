using System.Net;
using System.Reflection;
using System.Text.Json;
using EFStudio.Contracts;
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
        ITargetHost targetHost,
        CancellationToken cancellationToken = default
    )
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(options.Url);
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddSimpleConsole(console =>
        {
            console.SingleLine = true;
            console.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddSingleton(targetHost);
        builder.Services.AddSingleton<StudioAssetService>();

        var app = builder.Build();

        app.MapGet("/", () => Results.Redirect("/efstudio"));
        app.MapGet("/efstudio/api/health", () => Results.Ok(new { status = "ok", readOnly = true }));

        app.MapGet("/efstudio/api/contexts", (ITargetHost host) =>
            Results.Json(
                new DbContextListResponseContract(
                    host.GetAvailableContexts(),
                    host.GetSelectedContextName()
                ),
                JsonOptions
            )
        );

        app.MapPost(
            "/efstudio/api/contexts/select",
            async (HttpContext httpContext, ITargetHost host) =>
            {
                var request = await httpContext.Request.ReadFromJsonAsync<SelectDbContextRequestContract>(
                    JsonOptions,
                    httpContext.RequestAborted
                );

                if (request == null || !host.SelectContext(request.ContextName))
                {
                    return Results.NotFound(
                        new ErrorResponseContract(
                            $"The DbContext '{request?.ContextName}' could not be selected."
                        )
                    );
                }

                return Results.Json(
                    new DbContextListResponseContract(
                        host.GetAvailableContexts(),
                        host.GetSelectedContextName()
                    ),
                    JsonOptions
                );
            }
        );

        app.MapGet(
            "/efstudio/api/schema",
            async (HttpContext httpContext, ITargetHost host) =>
            {
                try
                {
                    return Results.Json(
                        await host.GetSchemaAsync(GetContextName(httpContext), httpContext.RequestAborted),
                        JsonOptions
                    );
                }
                catch (Exception exception)
                {
                    return ToErrorResult(exception);
                }
            }
        );

        app.MapGet(
            "/efstudio/api/tables",
            async (HttpContext httpContext, ITargetHost host) =>
            {
                try
                {
                    return Results.Json(
                        await host.GetSchemaAsync(GetContextName(httpContext), httpContext.RequestAborted),
                        JsonOptions
                    );
                }
                catch (Exception exception)
                {
                    return ToErrorResult(exception);
                }
            }
        );

        app.MapGet(
            "/efstudio/api/data",
            async (HttpContext httpContext, ITargetHost host) =>
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
                    var page = await host.GetTablePageAsync(
                        GetContextName(httpContext),
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
                    return ToErrorResult(exception);
                }
            }
        );

        app.MapPost(
            "/efstudio/api/data",
            async (HttpContext httpContext, ITargetHost host) =>
            {
                try
                {
                    var request = await httpContext.Request.ReadFromJsonAsync<CreateRecordsRequestContract>(
                        JsonOptions,
                        httpContext.RequestAborted
                    );

                    if (request == null)
                    {
                        return Results.BadRequest(
                            new ErrorResponseContract("Provide a create request before creating records.")
                        );
                    }

                    var result = await host.CreateRecordsAsync(
                        GetContextName(httpContext),
                        request,
                        httpContext.RequestAborted
                    );

                    return Results.Json(result, JsonOptions);
                }
                catch (JsonException)
                {
                    return Results.BadRequest(
                        new ErrorResponseContract("Provide a valid create request before creating records.")
                    );
                }
                catch (Exception exception)
                {
                    return ToErrorResult(exception);
                }
            }
        );

        app.MapPut(
            "/efstudio/api/data",
            async (HttpContext httpContext, ITargetHost host) =>
            {
                try
                {
                    var request = await httpContext.Request.ReadFromJsonAsync<UpdateRecordsRequestContract>(
                        JsonOptions,
                        httpContext.RequestAborted
                    );

                    if (request == null)
                    {
                        return Results.BadRequest(
                            new ErrorResponseContract("Provide an update request before updating records.")
                        );
                    }

                    var result = await host.UpdateRecordsAsync(
                        GetContextName(httpContext),
                        request,
                        httpContext.RequestAborted
                    );

                    return Results.Json(result, JsonOptions);
                }
                catch (Exception exception)
                {
                    return ToErrorResult(exception);
                }
            }
        );

        app.MapDelete(
            "/efstudio/api/data",
            async (HttpContext httpContext, ITargetHost host) =>
            {
                try
                {
                    var request = await httpContext.Request.ReadFromJsonAsync<DeleteRecordsRequestContract>(
                        JsonOptions,
                        httpContext.RequestAborted
                    );

                    if (request == null)
                    {
                        return Results.BadRequest(
                            new ErrorResponseContract("Provide a delete request before deleting records.")
                        );
                    }

                    var result = await host.DeleteRecordsAsync(
                        GetContextName(httpContext),
                        request,
                        httpContext.RequestAborted
                    );

                    return Results.Json(result, JsonOptions);
                }
                catch (Exception exception)
                {
                    return ToErrorResult(exception);
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
        return new StudioServerHandle(app, GetBaseUri(app, options));
    }

    private static string? GetContextName(HttpContext httpContext)
    {
        var contextName = httpContext.Request.Query["context"].ToString();
        return string.IsNullOrWhiteSpace(contextName) ? null : contextName;
    }

    private static IResult ToErrorResult(Exception exception)
    {
        var effectiveException = exception is TargetInvocationException && exception.InnerException != null
            ? exception.InnerException
            : exception.GetBaseException();

        if (effectiveException is TargetHostException hostException)
        {
            return Results.Json(
                new ErrorResponseContract(hostException.Message),
                JsonOptions,
                statusCode: hostException.StatusCode
            );
        }

        if (effectiveException is EFStudioRequestException requestException)
        {
            return Results.Json(
                new ErrorResponseContract(requestException.Message),
                JsonOptions,
                statusCode: requestException.StatusCode
            );
        }

        var statusCode = effectiveException is InvalidOperationException
            ? HttpStatusCode.BadRequest
            : HttpStatusCode.InternalServerError;

        return Results.Json(
            new ErrorResponseContract(effectiveException.Message),
            JsonOptions,
            statusCode: (int)statusCode
        );
    }

    private static int ParseInt(string? value, int fallback)
    {
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static Uri GetBaseUri(WebApplication app, StudioServerOptions options)
    {
        var address = app.Urls.FirstOrDefault();
        return Uri.TryCreate(address, UriKind.Absolute, out var baseUri)
            ? baseUri
            : options.BaseUri;
    }
}
