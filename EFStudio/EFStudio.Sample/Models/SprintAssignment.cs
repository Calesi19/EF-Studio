namespace EFStudio.Sample.Sqlite.Models;

public class SprintAssignment
{
    public int Id { get; set; }
    public int SprintId { get; set; }
    public long WorkItemId { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public short SortOrder { get; set; }
    public Sprint? Sprint { get; set; }
    public WorkItem? WorkItem { get; set; }
}
