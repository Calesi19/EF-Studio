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
            .FirstOrDefault(t => t.GetTableName() == request.TableName);

        if (entityType == null)
        {
            _logger.LogWarning(
                "EFStudio table data requested for unknown table {TableName}.",
                request.TableName
            );
            return null;
        }

        _logger.LogInformation("Loading EFStudio table data for {TableName}.", request.TableName);

        var tableIdentifier = StoreObjectIdentifier.Table(request.TableName, entityType.GetSchema());
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

        return new TableDataResponseContract(request.TableName, rows);
    }
}

public static class DbContextExtensions
{
    public static IQueryable<object> Query(this DbContext context, Type entityType)
    {
        return (IQueryable<object>)
            context
                .GetType()
                .GetMethod("Set", [])!
                .MakeGenericMethod(entityType)
                .Invoke(context, null)!;
    }
}
