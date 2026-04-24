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
            .Select(entityType =>
            {
                // Get the actual table name in the DB
                var tableName = entityType.GetTableName() ?? entityType.DisplayName();
                var tableIdentifier = StoreObjectIdentifier.Table(
                    tableName,
                    entityType.GetSchema()
                );

                return new TableInfo
                {
                    Name = tableName,
                    Columns = entityType
                        .GetProperties()
                        .Select(p =>
                        {
                            var fk = p.GetContainingForeignKeys().FirstOrDefault();
                            return new ColumnInfo
                            {
                                Name = p.GetColumnName(tableIdentifier) ?? p.Name,
                                DataType = p.GetColumnType(),
                                IsPrimaryKey = p.IsPrimaryKey(),
                                IsForeignKey = fk != null,
                                ForeignKeyTable = fk?.PrincipalEntityType.GetTableName(),
                                IsNullable = p.IsNullable,
                            };
                        })
                        .ToList(),
                };
            })
            .ToList();
    }
}
