namespace EFStudio.Core.Contracts;

public record TableInfoContract(
    string Key,
    string Name,
    string ModelName,
    string? Schema,
    IReadOnlyList<ColumnInfoContract> Columns
);
