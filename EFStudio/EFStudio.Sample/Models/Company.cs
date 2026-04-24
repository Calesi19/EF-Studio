namespace EFStudio.Sample.Sqlite.Models;

public class Company
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Industry { get; set; }
    public DateOnly FoundedOn { get; set; }
    public decimal AnnualRevenue { get; set; }
    public bool IsActive { get; set; }
    public string? SupportEmail { get; set; }
    public List<Office> Offices { get; set; } = [];
    public List<Department> Departments { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
    public List<Post> Posts { get; set; } = [];
    public List<Invoice> Invoices { get; set; } = [];
    public List<Asset> Assets { get; set; } = [];
    public List<AuditLog> AuditLogs { get; set; } = [];
}
