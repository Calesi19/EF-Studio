namespace EFStudio.Contracts;

public record DeleteRecordsResponseContract(string TableKey, int DeletedCount);
