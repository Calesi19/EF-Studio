namespace EFStudio.Sample.Sqlite.Models;

public class TeamMembership
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Guid EmployeeId { get; set; }
    public required string Role { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    public bool IsLead { get; set; }
    public Team? Team { get; set; }
    public Employee? Employee { get; set; }
}
