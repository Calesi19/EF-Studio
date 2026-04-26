using EFStudio.Contracts;
using EFStudio.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

internal sealed class TestDbContextCatalog(
    IReadOnlyDictionary<string, Func<DbContext>> registrations,
    string? selectedContextName = null
) : ITargetHost
{
    private readonly IReadOnlyDictionary<string, Func<DbContext>> _registrations = registrations;
    private readonly SchemaService _schemaService = new(NullLogger<SchemaService>.Instance);
    private readonly DataService _dataService = new(NullLogger<DataService>.Instance);
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

    public Task<IReadOnlyList<TableInfoContract>> GetSchemaAsync(
        string? contextName,
        CancellationToken cancellationToken
    )
    {
        using var context = CreateContext(contextName);
        return Task.FromResult(_schemaService.GetSchema(context));
    }

    public async Task<TablePageResponseContract?> GetTablePageAsync(
        string? contextName,
        TablePageRequestContract request,
        CancellationToken cancellationToken
    )
    {
        await using var context = CreateContext(contextName);
        return await _dataService.GetTablePageAsync(context, request, cancellationToken);
    }

    public Task<UpdateRecordsResponseContract> UpdateRecordsAsync(
        string? contextName,
        UpdateRecordsRequestContract request,
        CancellationToken cancellationToken
    )
    {
        return ExecuteAsync(
            contextName,
            context => _dataService.UpdateRecordsAsync(context, request, cancellationToken)
        );
    }

    public Task<DeleteRecordsResponseContract> DeleteRecordsAsync(
        string? contextName,
        DeleteRecordsRequestContract request,
        CancellationToken cancellationToken
    )
    {
        return ExecuteAsync(
            contextName,
            context => _dataService.DeleteRecordsAsync(context, request, cancellationToken)
        );
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
    }

    public static TestDbContextCatalog CreateSingle<TContext>(
        string contextName,
        Func<TContext> createContext
    )
        where TContext : DbContext
    {
        return new TestDbContextCatalog(
            new Dictionary<string, Func<DbContext>>
            {
                [contextName] = () => createContext(),
            },
            contextName
        );
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        string? contextName,
        Func<DbContext, Task<TResult>> action
    )
    {
        await using var context = CreateContext(contextName);
        return await action(context);
    }

    private DbContext CreateContext(string? contextName)
    {
        var effectiveContextName = contextName ?? _selectedContextName;
        if (string.IsNullOrWhiteSpace(effectiveContextName))
        {
            throw new InvalidOperationException("No DbContext is currently selected.");
        }

        if (!_registrations.TryGetValue(effectiveContextName, out var contextFactory))
        {
            throw new InvalidOperationException(
                $"EFStudio could not find a DbContext named '{effectiveContextName}'."
            );
        }

        return contextFactory();
    }
}
