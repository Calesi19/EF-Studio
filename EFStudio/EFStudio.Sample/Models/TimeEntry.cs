namespace EFStudio.Sample.Sqlite.Models;

public class TimeEntry
{
    public long Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public long? WorkItemId { get; set; }
    public DateOnly LoggedOn { get; set; }
    public decimal Hours { get; set; }
    public required string Description { get; set; }
    public TimeEntrySource Source { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Employee? Employee { get; set; }
    public Project? Project { get; set; }
    public WorkItem? WorkItem { get; set; }
}
