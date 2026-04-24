using EFStudio.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace EFStudio.Core.Services;

public class DataService
{
    private readonly ILogger<DataService> _logger;

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
