using EFStudio.Core.Contracts;
using EFStudio.Core.Services;
using Microsoft.EntityFrameworkCore;

internal sealed class TestDbContextCatalog(
    IReadOnlyDictionary<string, Func<DbContextLease>> registrations,
    string? selectedContextName = null
) : IDbContextCatalog
{
    private readonly IReadOnlyDictionary<string, Func<DbContextLease>> _registrations = registrations;
    private string? _selectedContextName = selectedContextName
        ?? registrations.Keys.FirstOrDefault();

    public IReadOnlyList<DbContextInfoContract> GetAvailableContexts()
    {
        return _registrations.Keys
            .Select(contextName => new DbContextInfoContract(
                contextName,
                contextName,
                contextName == _selectedContextName,
                _registrations.Count == 1 && contextName == _selectedContextName,
                IsAvailable: true,
                CreatedByDesignTimeFactory: false,
                ActivationError: null
            ))
            .ToList();
    }

    public string? GetSelectedContextName() => _selectedContextName;

    public bool SelectContext(string contextName)
    {
        if (!_registrations.ContainsKey(contextName))
        {
            return false;
        }

        _selectedContextName = contextName;
        return true;
    }

    public ValueTask<DbContextLease> LeaseDbContextAsync(
        string? contextName = null,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var effectiveContextName = contextName ?? _selectedContextName;
        if (string.IsNullOrWhiteSpace(effectiveContextName))
        {
            throw new InvalidOperationException("No DbContext is currently selected.");
        }

        if (!_registrations.TryGetValue(effectiveContextName, out var leaseFactory))
        {
            throw new InvalidOperationException(
                $"EFStudio could not find a DbContext named '{effectiveContextName}'."
            );
        }

        return ValueTask.FromResult(leaseFactory());
    }

    public static TestDbContextCatalog CreateSingle<TContext>(
        string contextName,
        Func<TContext> createContext
    )
        where TContext : DbContext
    {
        return new TestDbContextCatalog(
            new Dictionary<string, Func<DbContextLease>>
            {
                [contextName] = () => new DbContextLease(createContext()),
            },
            contextName
        );
    }
}
