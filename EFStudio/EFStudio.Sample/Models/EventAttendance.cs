namespace EFStudio.Sample.Sqlite.Models;

public class EventAttendance
{
    public int Id { get; set; }
    public int CompanyEventId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime RegisteredAtUtc { get; set; }
    public DateTimeOffset? CheckedInAtUtc { get; set; }
    public AttendanceStatus Status { get; set; }
    public CompanyEvent? CompanyEvent { get; set; }
    public Employee? Employee { get; set; }
}
