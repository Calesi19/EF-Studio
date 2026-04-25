using EFStudio.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Core.Services;

public interface IDbContextCatalog
{
    IReadOnlyList<DbContextInfoContract> GetAvailableContexts();
    string? GetSelectedContextName();
    bool SelectContext(string contextName);
    ValueTask<DbContextLease> LeaseDbContextAsync(
        string? contextName = null,
        CancellationToken cancellationToken = default
    );
}
