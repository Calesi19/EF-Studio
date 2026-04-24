namespace EFStudio.Core.Contracts;

public record TableInfoContract(string Name, IReadOnlyList<ColumnInfoContract> Columns);
