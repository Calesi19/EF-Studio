namespace EFStudio.Contracts;

public interface ITargetHost : IDisposable, IAsyncDisposable
{
    IReadOnlyList<DbContextInfoContract> GetAvailableContexts();
    string? GetSelectedContextName();
    bool SelectContext(string contextName);
    Task<IReadOnlyList<TableInfoContract>> GetSchemaAsync(
        string? contextName,
        CancellationToken cancellationToken
    );
    Task<TablePageResponseContract?> GetTablePageAsync(
        string? contextName,
        TablePageRequestContract request,
        CancellationToken cancellationToken
    );
    Task<CreateRecordsResponseContract> CreateRecordsAsync(
        string? contextName,
        CreateRecordsRequestContract request,
        CancellationToken cancellationToken
    );
    Task<UpdateRecordsResponseContract> UpdateRecordsAsync(
        string? contextName,
        UpdateRecordsRequestContract request,
        CancellationToken cancellationToken
    );
    Task<DeleteRecordsResponseContract> DeleteRecordsAsync(
        string? contextName,
        DeleteRecordsRequestContract request,
        CancellationToken cancellationToken
    );
}
