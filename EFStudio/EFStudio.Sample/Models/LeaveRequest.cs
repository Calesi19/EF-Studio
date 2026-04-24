namespace EFStudio.Sample.Sqlite.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? ApproverId { get; set; }
    public required string LeaveType { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public LeaveRequestStatus Status { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string? Reason { get; set; }
    public bool IsHalfDay { get; set; }
    public Employee? Employee { get; set; }
    public Employee? Approver { get; set; }
}
