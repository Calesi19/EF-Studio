namespace EFStudio.Core.Contracts;

public record TableDataResponseContract(
    string Key,
    string Name,
    string? Schema,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows
);
