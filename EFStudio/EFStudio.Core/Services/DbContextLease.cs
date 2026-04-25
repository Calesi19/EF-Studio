using Microsoft.EntityFrameworkCore;

namespace EFStudio.Core.Services;

public sealed class DbContextLease : IDisposable, IAsyncDisposable
{
    private readonly object? _owner;

    public DbContextLease(DbContext context, object? owner = null)
    {
        Context = context;
        _owner = owner ?? context;
    }

    public DbContext Context { get; }

    public void Dispose()
    {
        if (_owner is IDisposable disposable)
        {
            disposable.Dispose();
            return;
        }

        if (_owner is IAsyncDisposable asyncDisposable)
        {
            asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_owner is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
            return;
        }

        if (_owner is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
