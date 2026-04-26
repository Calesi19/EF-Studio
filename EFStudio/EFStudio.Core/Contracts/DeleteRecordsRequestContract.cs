using System.Text.Json;

namespace EFStudio.Contracts;

public record DeleteRecordsRequestContract(
    string TableKey,
    IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> Keys
);
