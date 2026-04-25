using EFStudio.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Core.Services;

public interface IDataService
{
    Task<TableDataResponseContract?> GetTableDataAsync(
        DbContext dbContext,
        TableDataRequestContract request,
        CancellationToken cancellationToken
    );

    Task<TablePageResponseContract?> GetTablePageAsync(
        DbContext dbContext,
        TablePageRequestContract request,
        CancellationToken cancellationToken
    );

    Task<DeleteRecordsResponseContract> DeleteRecordsAsync(
        DbContext dbContext,
        DeleteRecordsRequestContract request,
        CancellationToken cancellationToken
    );

    Task<UpdateRecordsResponseContract> UpdateRecordsAsync(
        DbContext dbContext,
        UpdateRecordsRequestContract request,
        CancellationToken cancellationToken
    );
}
