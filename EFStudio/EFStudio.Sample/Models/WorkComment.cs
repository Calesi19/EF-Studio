namespace EFStudio.Sample.Sqlite.Models;

public class WorkComment
{
    public long Id { get; set; }
    public long WorkItemId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Body { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsInternal { get; set; }
    public WorkItem? WorkItem { get; set; }
    public Employee? Author { get; set; }
}
