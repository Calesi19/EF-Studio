namespace EFStudio.Contracts;

public sealed record TablePageResponseContract(
    string Key,
    string Name,
    string? Schema,
    int Page,
    int PageSize,
    int TotalRows,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows
);
