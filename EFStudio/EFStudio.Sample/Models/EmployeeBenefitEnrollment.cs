namespace EFStudio.Sample.Sqlite.Models;

public class EmployeeBenefitEnrollment
{
    public int Id { get; set; }
    public int BenefitPlanId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly EffectiveOn { get; set; }
    public DateOnly? EndedOn { get; set; }
    public required string CoverageLevel { get; set; }
    public bool IsPrimary { get; set; }
    public BenefitPlan? BenefitPlan { get; set; }
    public Employee? Employee { get; set; }
}
