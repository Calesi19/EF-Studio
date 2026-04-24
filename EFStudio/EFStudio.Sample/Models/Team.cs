namespace EFStudio.Sample.Sqlite.Models;

public class Team
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? FocusArea { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public Company? Company { get; set; }
    public Department? Department { get; set; }
    public List<TeamMembership> Members { get; set; } = [];
}
