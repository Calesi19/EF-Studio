namespace EFStudio.Core.Contracts;

public sealed record DbContextListResponseContract(
    IReadOnlyList<DbContextInfoContract> Contexts,
    string? SelectedContext
);
