using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFStudio.Core.Services;

internal static class TableKeyFactory
{
    public static string Create(string? schema, string tableName)
    {
        return string.IsNullOrWhiteSpace(schema) ? tableName : $"{schema}.{tableName}";
    }

    public static string Create(IEntityType entityType)
    {
        var tableName = entityType.GetTableName() ?? entityType.DisplayName();
        return Create(entityType.GetSchema(), tableName);
    }
}
