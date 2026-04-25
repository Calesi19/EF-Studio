namespace EFStudio.Core.Contracts;

public sealed record TablePageRequestContract(
    string TableKey,
    int Page = 1,
    int PageSize = 50,
    string? Filter = null,
    string? SortColumn = null,
    string? SortDirection = null
);
