namespace EFStudio.Sample.Sqlite.Models;

public class WorkItem
{
    public long Id { get; set; }
    public int ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid ReporterId { get; set; }
    public long? ParentWorkItemId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public WorkItemType Type { get; set; }
    public WorkItemState State { get; set; }
    public byte? StoryPoints { get; set; }
    public decimal OriginalEstimateHours { get; set; }
    public decimal RemainingHours { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public Project? Project { get; set; }
    public Employee? Assignee { get; set; }
    public Employee? Reporter { get; set; }
    public WorkItem? ParentWorkItem { get; set; }
    public List<WorkItem> ChildWorkItems { get; set; } = [];
    public List<WorkComment> Comments { get; set; } = [];
    public List<WorkItemTag> WorkItemTags { get; set; } = [];
}
