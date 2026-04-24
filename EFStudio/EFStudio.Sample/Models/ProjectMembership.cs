namespace EFStudio.Sample.Sqlite.Models;

public class ProjectMembership
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public required string Role { get; set; }
    public float AllocationPercent { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    public bool IsPrimaryContact { get; set; }
    public Project? Project { get; set; }
    public Employee? Employee { get; set; }
}
