namespace EFStudio.Sample.Sqlite.Models;

public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string HexColor { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<WorkItemTag> WorkItemTags { get; set; } = [];
}
