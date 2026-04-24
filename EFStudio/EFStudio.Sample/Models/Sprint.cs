namespace EFStudio.Sample.Sqlite.Models;

public class Sprint
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public required string Name { get; set; }
    public string? Goal { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public SprintGoalStatus GoalStatus { get; set; }
    public decimal CapacityHours { get; set; }
    public Project? Project { get; set; }
    public List<SprintAssignment> Assignments { get; set; } = [];
}
