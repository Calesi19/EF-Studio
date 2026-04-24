namespace EFStudio.Sample.Sqlite.Models;

public class Project
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public double EstimatedHours { get; set; }
    public decimal SpentBudget { get; set; }
    public ProjectStatus Status { get; set; }
    public short Priority { get; set; }
    public bool IsBillable { get; set; }
    public Company? Company { get; set; }
    public Department? Department { get; set; }
    public List<ProjectMembership> Memberships { get; set; } = [];
    public List<WorkItem> WorkItems { get; set; } = [];
    public List<Invoice> Invoices { get; set; } = [];
}
