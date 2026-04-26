namespace EFStudio.Contracts;

public sealed record DbContextListResponseContract(
    IReadOnlyList<DbContextInfoContract> Contexts,
    string? SelectedContext
);
