using System.Globalization;
using System.Text.Json;
using EFStudio.Core.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace EFStudio.Core.Services;

public class DataService
{
    private readonly ILogger<DataService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public DataService(ILogger<DataService> logger) => _logger = logger;

    public async Task<TableDataResponseContract?> GetTableDataAsync(
        DbContext dbContext,
        TableDataRequestContract request,
        CancellationToken cancellationToken
    )
    {
        var entityType = dbContext
            .Model.GetEntityTypes()
            .FirstOrDefault(t => TableKeyFactory.Create(t) == request.TableKey);

        if (entityType == null)
        {
            _logger.LogWarning(
                "EFStudio table data requested for unknown table {TableKey}.",
                request.TableKey
            );
            return null;
        }

        var tableName = entityType.GetTableName() ?? entityType.DisplayName();
        var schema = entityType.GetSchema();
        var tableKey = TableKeyFactory.Create(schema, tableName);

        _logger.LogInformation("Loading EFStudio table data for {TableKey}.", tableKey);

        var tableIdentifier = StoreObjectIdentifier.Table(tableName, schema);
        var properties = entityType.GetProperties().ToList();

        var results = await dbContext
            .Query(entityType.ClrType)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rows = results
            .Select(item =>
                (IReadOnlyDictionary<string, object?>)
                    properties.ToDictionary(
                        property => property.GetColumnName(tableIdentifier) ?? property.Name,
                        property => property.GetGetter().GetClrValue(item)
                    )
            )
            .ToList();

        return new TableDataResponseContract(tableKey, tableName, schema, rows);
    }

    public async Task<DeleteRecordsResponseContract> DeleteRecordsAsync(
        DbContext dbContext,
        DeleteRecordsRequestContract request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.TableKey))
        {
            throw new EFStudioRequestException(
                StatusCodes.Status400BadRequest,
                "Choose a table before deleting records."
            );
        }

        if (request.Keys.Count == 0)
        {
            throw new EFStudioRequestException(
                StatusCodes.Status400BadRequest,
                "Select at least one record to delete."
            );
        }

        var entityType = dbContext
            .Model.GetEntityTypes()
            .FirstOrDefault(t => TableKeyFactory.Create(t) == request.TableKey);

        if (entityType == null)
        {
            throw new EFStudioRequestException(
                StatusCodes.Status404NotFound,
                $"The table '{request.TableKey}' could not be found."
            );
        }

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey == null || primaryKey.Properties.Count == 0)
        {
            throw new EFStudioRequestException(
                StatusCodes.Status400BadRequest,
                $"The table '{request.TableKey}' does not support delete operations because it has no primary key."
            );
        }

        var tableName = entityType.GetTableName() ?? entityType.DisplayName();
        var schema = entityType.GetSchema();
        var tableKey = TableKeyFactory.Create(schema, tableName);
        var tableIdentifier = StoreObjectIdentifier.Table(tableName, schema);
        var keyColumns = primaryKey.Properties
            .Select(property => new KeyColumn(
                property,
                property.GetColumnName(tableIdentifier) ?? property.Name
            ))
            .ToList();

        _logger.LogInformation(
            "Deleting {RecordCount} EFStudio records from {TableKey}.",
            request.Keys.Count,
            tableKey
        );

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var keyValues in request.Keys)
            {
                var orderedKeyValues = keyColumns
                    .Select(keyColumn => ConvertKeyValue(keyColumn, keyValues, tableKey))
                    .ToArray();

                var entity = await dbContext.FindAsync(
                    entityType.ClrType,
                    orderedKeyValues,
                    cancellationToken
                );

                if (entity == null)
                {
                    throw new EFStudioRequestException(
                        StatusCodes.Status404NotFound,
                        $"A selected record in '{tableKey}' could not be found."
                    );
                }

                dbContext.Remove(entity);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new DeleteRecordsResponseContract(tableKey, request.Keys.Count);
        }
        catch (EFStudioRequestException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(
                exception,
                "Delete failed for EFStudio table {TableKey}.",
                tableKey
            );

            throw new EFStudioRequestException(
                StatusCodes.Status409Conflict,
                $"EFStudio could not delete the selected records from '{tableKey}' because they are still referenced by related data."
            );
        }
        catch (InvalidOperationException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogWarning(
                exception,
                "Delete validation failed for EFStudio table {TableKey}.",
                tableKey
            );

            throw new EFStudioRequestException(
                StatusCodes.Status409Conflict,
                $"EFStudio could not delete the selected records from '{tableKey}' because they are still referenced by related data."
            );
        }
    }

    private object ConvertKeyValue(
        KeyColumn keyColumn,
        IReadOnlyDictionary<string, JsonElement> keyValues,
        string tableKey
    )
    {
        if (!keyValues.TryGetValue(keyColumn.ColumnName, out var jsonValue))
        {
            throw new EFStudioRequestException(
                StatusCodes.Status400BadRequest,
                $"Delete requests for '{tableKey}' must include the primary key column '{keyColumn.ColumnName}'."
            );
        }

        var targetType = keyColumn.Property.ClrType;
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (jsonValue.ValueKind == JsonValueKind.Null)
        {
            if (Nullable.GetUnderlyingType(targetType) != null)
            {
                return null!;
            }

            throw new EFStudioRequestException(
                StatusCodes.Status400BadRequest,
                $"The primary key column '{keyColumn.ColumnName}' for '{tableKey}' cannot be null."
            );
        }

        try
        {
            if (underlyingType.IsEnum)
            {
                return jsonValue.ValueKind == JsonValueKind.String
                    ? Enum.Parse(underlyingType, jsonValue.GetString()!, ignoreCase: true)
                    : Enum.ToObject(underlyingType, jsonValue.Deserialize(typeof(int), JsonOptions)!);
            }

            if (underlyingType == typeof(Guid))
            {
                return jsonValue.ValueKind == JsonValueKind.String
                    ? Guid.Parse(jsonValue.GetString()!)
                    : Guid.Parse(jsonValue.ToString());
            }

            if (underlyingType == typeof(DateOnly))
            {
                return DateOnly.Parse(jsonValue.GetString()!, CultureInfo.InvariantCulture);
            }

            if (underlyingType == typeof(TimeOnly))
            {
                return TimeOnly.Parse(jsonValue.GetString()!, CultureInfo.InvariantCulture);
            }

            var value = jsonValue.Deserialize(underlyingType, JsonOptions);
            if (value == null)
            {
                throw new InvalidOperationException("Primary key value deserialized to null.");
            }

            return value;
        }
        catch (Exception exception) when (
            exception is FormatException
                or InvalidOperationException
                or JsonException
                or OverflowException
                or ArgumentException
        )
        {
            throw new EFStudioRequestException(
                StatusCodes.Status400BadRequest,
                $"The primary key value for '{keyColumn.ColumnName}' in '{tableKey}' is invalid."
            );
        }
    }

    private sealed record KeyColumn(IProperty Property, string ColumnName);
}

public static class DbContextExtensions
{
    public static IQueryable<object> Query(this DbContext context, Type entityType)
    {
        return (IQueryable<object>)
            context
                .GetType()
                .GetMethod("Set", Type.EmptyTypes)!
                .MakeGenericMethod(entityType)
                .Invoke(context, null)!;
    }
}
