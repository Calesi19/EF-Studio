using System.Text.Json;

namespace EFStudio.Core.Contracts;

public record DeleteRecordsRequestContract(
    string TableKey,
    IReadOnlyList<IReadOnlyDictionary<string, JsonElement>> Keys
);
