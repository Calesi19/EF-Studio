using System.Text.Json;

namespace EFStudio.Contracts;

public record CreateRecordsRequestContract(
    string TableKey,
    IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> Records
);
