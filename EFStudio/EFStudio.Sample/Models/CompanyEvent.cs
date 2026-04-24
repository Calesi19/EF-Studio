namespace EFStudio.Sample.Sqlite.Models;

public class CompanyEvent
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Guid? OfficeId { get; set; }
    public Guid? HostEmployeeId { get; set; }
    public required string Name { get; set; }
    public required string EventType { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public short? Capacity { get; set; }
    public bool IsVirtual { get; set; }
    public Company? Company { get; set; }
    public Office? Office { get; set; }
    public Employee? HostEmployee { get; set; }
    public List<EventAttendance> Attendees { get; set; } = [];
}
