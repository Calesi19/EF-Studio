using System.Dynamic;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Core.Services;

public class DataService
{
    public async Task<IEnumerable<object>> GetTableDataAsync(DbContext dbContext, string tableName)
    {
        var entityType = dbContext
            .Model.GetEntityTypes()
            .FirstOrDefault(t => t.GetTableName() == tableName);

        if (entityType == null)
            return [];

        var query = dbContext.Query(entityType.ClrType).AsNoTracking();

        return await query.Cast<object>().ToListAsync();
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
