namespace EFStudio.Contracts;

public sealed record DbContextInfoContract(
    string Name,
    string DisplayName,
    bool IsSelected,
    bool IsDefault,
    bool IsAvailable,
    bool CreatedByDesignTimeFactory,
    string? ActivationError
);
