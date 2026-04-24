namespace EFStudio.Sample.Sqlite.Models;

public class BenefitPlan
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public required string Name { get; set; }
    public required string Provider { get; set; }
    public required string PlanType { get; set; }
    public decimal EmployeeMonthlyCost { get; set; }
    public decimal EmployerMonthlyContribution { get; set; }
    public bool IsActive { get; set; }
    public Company? Company { get; set; }
    public List<EmployeeBenefitEnrollment> Enrollments { get; set; } = [];
}
