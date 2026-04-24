using EFStudio.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace EFStudio.Core.Services;

public class SchemaService
{
    private readonly ILogger<SchemaService> _logger;

    public SchemaService(ILogger<SchemaService> logger) => _logger = logger;

    public IReadOnlyList<TableInfoContract> GetSchema(DbContext context)
    {
        _logger.LogInformation("Loading EFStudio schema for context {DbContextType}.", context.GetType().Name);

        return context
            .Model.GetEntityTypes()
            .Select(entityType =>
            {
                var tableName = entityType.GetTableName() ?? entityType.DisplayName();
                var schema = entityType.GetSchema();
                var tableIdentifier = StoreObjectIdentifier.Table(tableName, schema);
                var tableKey = TableKeyFactory.Create(schema, tableName);

                return new TableInfoContract(
                    tableKey,
                    tableName,
                    schema,
                    entityType
                        .GetProperties()
                        .Select(property =>
                        {
                            var foreignKey = property.GetContainingForeignKeys().FirstOrDefault();

                            return new ColumnInfoContract(
                                property.GetColumnName(tableIdentifier) ?? property.Name,
                                property.GetColumnType() ?? property.ClrType.Name,
                                property.IsPrimaryKey(),
                                property.IsNullable,
                                foreignKey != null,
                                foreignKey == null
                                    ? null
                                    : TableKeyFactory.Create(foreignKey.PrincipalEntityType)
                            );
                        })
                        .ToList()
                );
            })
            .ToList();
    }
}
