namespace EFStudio.Sample.Sqlite.Models;

public class AuditLog
{
    public long Id { get; set; }
    public int? CompanyId { get; set; }
    public Guid? EmployeeId { get; set; }
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public AuditAction Action { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? IpAddress { get; set; }
    public Guid CorrelationId { get; set; }
    public required string ChangesJson { get; set; }
    public bool Success { get; set; }
    public Company? Company { get; set; }
    public Employee? Employee { get; set; }
}
