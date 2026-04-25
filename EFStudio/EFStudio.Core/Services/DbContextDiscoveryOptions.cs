namespace EFStudio.Core.Services;

public sealed record DbContextDiscoveryOptions(
    string WorkingDirectory,
    string? ProjectPath = null,
    string? StartupProjectPath = null
);
