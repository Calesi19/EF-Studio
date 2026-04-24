namespace EFStudio.Core.Contracts;

public record ColumnInfoContract(
    string Name,
    string DataType,
    bool IsPrimaryKey,
    bool IsNullable,
    bool IsForeignKey,
    string? ForeignKeyTable
);
