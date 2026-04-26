namespace EFStudio.Contracts;

public record ColumnInfoContract(
    string Name,
    string DataType,
    bool IsPrimaryKey,
    bool IsNullable,
    bool IsForeignKey,
    string? ForeignKeyTable,
    bool IsGeneratedOnAdd,
    bool IsEditableOnCreate
);
