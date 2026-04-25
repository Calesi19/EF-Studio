namespace EFStudio.Core.Contracts;

public record DeleteRecordsResponseContract(string TableKey, int DeletedCount);
