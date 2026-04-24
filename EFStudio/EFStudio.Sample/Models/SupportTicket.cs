namespace EFStudio.Sample.Sqlite.Models;

public class SupportTicket
{
    public long Id { get; set; }
    public int CompanyId { get; set; }
    public int CustomerId { get; set; }
    public int? ProjectId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    public required string Subject { get; set; }
    public string? Description { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public byte? SatisfactionScore { get; set; }
    public Company? Company { get; set; }
    public Customer? Customer { get; set; }
    public Project? Project { get; set; }
    public Employee? AssignedEmployee { get; set; }
    public List<SupportTicketComment> Comments { get; set; } = [];
}
