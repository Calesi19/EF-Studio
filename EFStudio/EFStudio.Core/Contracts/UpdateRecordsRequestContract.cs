using System.Text.Json;

namespace EFStudio.Contracts;

public record UpdateRecordEntry(
    IReadOnlyDictionary<string, JsonElement> Keys,
    IReadOnlyDictionary<string, JsonElement> Values
);

public record UpdateRecordsRequestContract(
    string TableKey,
    IReadOnlyList<UpdateRecordEntry> Updates
);
