using EFStudio.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFStudio.Core.Services;

public class DataService
{
    public async Task<TableDataResponse> GetTableDataAsync(DbContext dbContext, string tableName)
    {
        var entityType = dbContext
            .Model.GetEntityTypes()
            .FirstOrDefault(t => t.GetTableName() == tableName);

        if (entityType == null)
            return new TableDataResponse { Name = tableName };

        var tableIdentifier = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());
        var properties = entityType.GetProperties().ToList();

        var results = await dbContext.Query(entityType.ClrType).AsNoTracking().ToListAsync();

        var rowsWithDbNames = new List<object>();

        foreach (var item in results)
        {
            var rowDict = new Dictionary<string, object?>();
            foreach (var prop in properties)
            {
                var dbColumnName = prop.GetColumnName(tableIdentifier) ?? prop.Name;
                rowDict[dbColumnName] = prop.GetGetter().GetClrValue(item);
            }
            rowsWithDbNames.Add(rowDict);
        }

        return new TableDataResponse { Name = tableName, Rows = rowsWithDbNames };
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
