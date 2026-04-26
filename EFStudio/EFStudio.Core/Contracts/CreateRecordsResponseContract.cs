namespace EFStudio.Contracts;

public record CreateRecordsResponseContract(
    string TableKey,
    int CreatedCount,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Records
);
