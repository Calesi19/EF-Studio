namespace EFStudio.Core.Contracts;

public record TableDataResponseContract(string Name, IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows);
