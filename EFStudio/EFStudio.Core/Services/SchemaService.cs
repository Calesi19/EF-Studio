using EFStudio.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFStudio.Core.Services;

public class SchemaExplorer
{
    public List<TableInfo> GetSchema(DbContext context)
    {
        return context
            .Model.GetEntityTypes()
            .Select(entityType => new TableInfo
            {
                Name = entityType.GetTableName() ?? entityType.DisplayName(),
                Columns = entityType
                    .GetProperties()
                    .Select(p => new ColumnInfo
                    {
                        Name =
                            p.GetColumnName(
                                StoreObjectIdentifier.Table(entityType.GetTableName()!, null)
                            ) ?? p.Name,
                        DataType = p.GetColumnType(),
                        IsPrimaryKey = p.IsPrimaryKey(),
                        IsNullable = p.IsNullable,
                    })
                    .ToList(),
            })
            .ToList();
    }
}
