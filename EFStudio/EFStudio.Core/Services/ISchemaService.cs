using EFStudio.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EFStudio.Core.Services;

public interface ISchemaService
{
    IReadOnlyList<TableInfoContract> GetSchema(DbContext context);
}
