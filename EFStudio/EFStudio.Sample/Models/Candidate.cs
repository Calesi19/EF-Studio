namespace EFStudio.Sample.Sqlite.Models;

public class Candidate
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public Guid? RecruiterEmployeeId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public CandidateStage Stage { get; set; }
    public string? Source { get; set; }
    public DateOnly AppliedOn { get; set; }
    public decimal? DesiredSalary { get; set; }
    public bool IsRemote { get; set; }
    public Company? Company { get; set; }
    public Department? Department { get; set; }
    public Employee? RecruiterEmployee { get; set; }
    public List<InterviewSession> Interviews { get; set; } = [];
}
