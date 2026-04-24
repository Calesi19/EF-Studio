namespace EFStudio.Core.Models;

public class TableDataResponse
{
    public string Name { get; set; } = string.Empty;
    public List<object> Rows { get; set; } = [];
}
