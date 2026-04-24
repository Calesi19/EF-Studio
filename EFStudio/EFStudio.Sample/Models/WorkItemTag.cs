namespace EFStudio.Sample.Sqlite.Models;

public class WorkItemTag
{
    public int Id { get; set; }
    public long WorkItemId { get; set; }
    public int TagId { get; set; }
    public WorkItem? WorkItem { get; set; }
    public Tag? Tag { get; set; }
}
