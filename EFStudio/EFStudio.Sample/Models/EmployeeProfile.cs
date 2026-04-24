namespace EFStudio.Sample.Sqlite.Models;

public class EmployeeProfile
{
    public int Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Biography { get; set; }
    public required string EmergencyContactName { get; set; }
    public required string EmergencyContactPhone { get; set; }
    public required string PreferredLanguage { get; set; }
    public bool NotifyByEmail { get; set; }
    public bool NotifyBySms { get; set; }
    public required string Theme { get; set; }
    public Employee? Employee { get; set; }
}
