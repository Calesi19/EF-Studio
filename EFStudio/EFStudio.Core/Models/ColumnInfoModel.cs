namespace EFStudio.Core.Models;

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
    public bool IsForeignKey { get; set; }
    public string? ForeignKeyTable { get; set; }
}
