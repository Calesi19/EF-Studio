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
    private const string ContextsPath = $"{RootPath}/api/contexts";
    private const string ContextSelectionPath = $"{RootPath}/api/contexts/select";
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
        StudioAssetService assetService,
        DbContext dbContext
    )
    {
        var path = context.Request.Path.Value ?? "";

        try
        {
            if (path.Equals(ContextsPath, StringComparison.OrdinalIgnoreCase))
            {
                var contextName = dbContext.GetType().Name;
                var payload = new DbContextListResponseContract(
                    new[]
                    {
                        new DbContextInfoContract(
                            contextName,
                            dbContext.GetType().FullName ?? contextName,
                            IsSelected: true,
                            IsDefault: true,
                            IsAvailable: true,
                            CreatedByDesignTimeFactory: false,
                            ActivationError: null
                        ),
                    },
                    contextName
                );

                await WriteJsonAsync(context, StatusCodes.Status200OK, payload);
                return;
            }

            if (
                path.Equals(ContextSelectionPath, StringComparison.OrdinalIgnoreCase)
                && HttpMethods.IsPost(context.Request.Method)
            )
            {
                var request = await JsonSerializer.DeserializeAsync<SelectDbContextRequestContract>(
                    context.Request.Body,
                    JsonOptions,
                    context.RequestAborted
                );

                var contextName = dbContext.GetType().Name;
                if (request == null || !string.Equals(request.ContextName, contextName, StringComparison.Ordinal))
                {
                    await WriteErrorAsync(
                        context,
                        StatusCodes.Status404NotFound,
                        $"The DbContext '{request?.ContextName}' could not be found."
                    );
                    return;
                }

                await WriteJsonAsync(
                    context,
                    StatusCodes.Status200OK,
                    new DbContextListResponseContract(
                        new[]
                        {
                            new DbContextInfoContract(
                                contextName,
                                dbContext.GetType().FullName ?? contextName,
                                IsSelected: true,
                                IsDefault: true,
                                IsAvailable: true,
                                CreatedByDesignTimeFactory: false,
                                ActivationError: null
                            ),
                        },
                        contextName
                    )
                );
                return;
            }

            if (path.Equals(SchemaPath, StringComparison.OrdinalIgnoreCase))
            {
                var schema = schemaService.GetSchema(dbContext);
                await WriteJsonAsync(context, StatusCodes.Status200OK, schema);
                return;
            }

            if (path.Equals(DataPath, StringComparison.OrdinalIgnoreCase))
            {
                if (HttpMethods.IsGet(context.Request.Method))
                {
                    var tableKey = context.Request.Query["table"].ToString();
                    if (string.IsNullOrEmpty(tableKey))
                    {
                        await WriteErrorAsync(
                            context,
                            StatusCodes.Status400BadRequest,
                            "Choose a table before requesting table data."
                        );
                        return;
                    }

                    var data = await dataService.GetTablePageAsync(
                        dbContext,
                        new TablePageRequestContract(
                            tableKey,
                            ParseInt(context.Request.Query["page"], 1),
                            ParseInt(context.Request.Query["pageSize"], 50),
                            context.Request.Query["filter"],
                            context.Request.Query["sortColumn"],
                            context.Request.Query["sortDirection"]
                        ),
                        context.RequestAborted
                    );

                    if (data == null)
                    {
                        await WriteErrorAsync(
                            context,
                            StatusCodes.Status404NotFound,
                            $"The table '{tableKey}' could not be found."
                        );
                        return;
                    }

                    await WriteJsonAsync(context, StatusCodes.Status200OK, data);
                    return;
                }

                if (HttpMethods.IsDelete(context.Request.Method))
                {
                    var request = await JsonSerializer.DeserializeAsync<DeleteRecordsRequestContract>(
                        context.Request.Body,
                        JsonOptions,
                        context.RequestAborted
                    );

                    if (request == null)
                    {
                        await WriteErrorAsync(
                            context,
                            StatusCodes.Status400BadRequest,
                            "Provide a delete request before deleting records."
                        );
                        return;
                    }

                    var result = await dataService.DeleteRecordsAsync(
                        dbContext,
                        request,
                        context.RequestAborted
                    );

                    await WriteJsonAsync(context, StatusCodes.Status200OK, result);
                    return;
                }

                if (HttpMethods.IsPut(context.Request.Method))
                {
                    var request = await JsonSerializer.DeserializeAsync<UpdateRecordsRequestContract>(
                        context.Request.Body,
                        JsonOptions,
                        context.RequestAborted
                    );

                    if (request == null)
                    {
                        await WriteErrorAsync(
                            context,
                            StatusCodes.Status400BadRequest,
                            "Provide an update request before updating records."
                        );
                        return;
                    }

                    var result = await dataService.UpdateRecordsAsync(
                        dbContext,
                        request,
                        context.RequestAborted
                    );

                    await WriteJsonAsync(context, StatusCodes.Status200OK, result);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                context.Response.Headers.Allow = $"{HttpMethods.Get}, {HttpMethods.Delete}, {HttpMethods.Put}";
                return;
            }

            if (path.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
            {
                await ServeEmbeddedFile(context, path, assetService);
                return;
            }

            await _next(context);
        }
        catch (EFStudioRequestException exception) when (
            path.Equals(DataPath, StringComparison.OrdinalIgnoreCase)
        )
        {
            await WriteErrorAsync(context, exception.StatusCode, exception.Message);
        }
        catch (JsonException) when (
            path.Equals(DataPath, StringComparison.OrdinalIgnoreCase) &&
            HttpMethods.IsDelete(context.Request.Method)
        )
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Provide a valid delete request before deleting records."
            );
        }
        catch (JsonException) when (
            path.Equals(DataPath, StringComparison.OrdinalIgnoreCase) &&
            HttpMethods.IsPut(context.Request.Method)
        )
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Provide a valid update request before updating records."
            );
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

    private async Task ServeEmbeddedFile(HttpContext context, string path, StudioAssetService assetService)
    {
        string resourcePath =
            path.Length <= RootPath.Length ? "index.html" : path.Substring(RootPath.Length + 1);

        if (!assetService.TryOpenAsset(resourcePath, out var stream, out var contentType) || stream == null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = contentType;
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

    private static int ParseInt(string? value, int fallback)
    {
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
