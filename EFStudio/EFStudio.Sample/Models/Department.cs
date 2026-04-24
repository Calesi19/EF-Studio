namespace EFStudio.Sample.Sqlite.Models;

public class Department
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public required string Name { get; set; }
    public required string CostCenter { get; set; }
    public decimal Budget { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Company? Company { get; set; }
    public List<Employee> Employees { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
}
