namespace EFStudio.Core.Contracts;

public record TableInfoContract(
    string Key,
    string Name,
    string? Schema,
    IReadOnlyList<ColumnInfoContract> Columns
);
