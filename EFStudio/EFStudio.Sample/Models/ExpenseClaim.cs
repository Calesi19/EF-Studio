namespace EFStudio.Sample.Sqlite.Models;

public class ExpenseClaim
{
    public int Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? ApproverId { get; set; }
    public required string Title { get; set; }
    public ExpenseStatus Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public Employee? Employee { get; set; }
    public Employee? Approver { get; set; }
    public List<ExpenseLine> Lines { get; set; } = [];
}
